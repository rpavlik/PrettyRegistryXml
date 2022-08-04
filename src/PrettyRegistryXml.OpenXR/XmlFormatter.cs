// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using PrettyRegistryXml.Core;
using PrettyRegistryXml.GroupedAlignment;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace PrettyRegistryXml.OpenXR
{
    /// <summary>
    /// OpenXR-specific policy for formatting XML.
    /// </summary>
    public class XmlFormatter : XmlFormatterBase
    {
        /// <inheritdoc />
        public override int IndentLevelWidth { get => 4; }

        /// <inheritdoc />
        public override string IndentChars { get => "    "; }

        /// <summary>
        /// Whether we should wrap the attributes of extension tags, a runtime preference set by the command line.
        /// </summary>
        private bool WrapExtensions { get; init; }

        /// <summary>
        /// Whether we should sort the return values, a runtime preference set by the command line, on by default.
        /// </summary>
        private bool SortReturnVals { get; init; }

        private readonly ReturnCodeSorter CodeSorter = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Formatting options, typically from command line.</param>
        public XmlFormatter(Options options)
        {
            WrapExtensions = options.WrapExtensions;
            SortReturnVals = options.SortCodes;
        }

        /// <summary>
        /// Checks an element category attribute to tell if it's a known "not single line" element.
        /// See also <see cref="ChildrenShouldBeSingleLine(XElement)"/> where this is used.
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>true if the element has a "category" attribute and the value is known to be one we don't want to single-line.</returns>
        private static bool IsCategoryNonSingleLine(XElement element)
        {
            var category = element.Attribute("category");
            return category != null
                   && (category.Value == "define" || category.Value == "struct");
        }

        /// <summary>
        /// A set of element names who always act as single-line containers.
        /// See also <see cref="ChildrenShouldBeSingleLine(XElement)"/> where this is used.
        /// </summary>
        private static readonly HashSet<string> singleLineContainers = new() { "member", "param", "proto" };

        /// <summary>
        /// Determine whether an element and its children should all be on a single line.
        /// </summary>
        /// <param name="e">An element</param>
        /// <returns>true if the element and its children should all be on a single line.</returns>
        private static bool ChildrenShouldBeSingleLine(XElement e)
        {
            // some containers should always have their children on a single line.
            if (singleLineContainers.Contains(e.Name.LocalName))
            {
                return true;
            }

            // some elements should only have their children on a single line when they're a specific usage:
            // lots of elts named "type" but they aren't all the same.
            return e.Name.LocalName == "type"
                   && e.Parent != null
                   && e.Parent.Name.LocalName == "types"
                   && !IsCategoryNonSingleLine(e);
        }

        /// <summary>
        /// Checks an element category attribute to tell if it's a bitmask.
        /// </summary>
        /// <param name="element">An element</param>
        /// <returns>true if the element defines a bitmask</returns>
        private static bool IsBitmask(XElement element)
        {
            var attr = element.Attribute("category");
            return element.Name == "type"
                   && attr != null
                   && attr.Value == "bitmask";
        }

        private static bool IsEnum(XElement element) => element.Name == "enum";

        /// <inheritdoc />
        public override int ComputeLevelAdjust(XNode node)
        {
            var extensionsInAncestors = (from el in node.Ancestors()
                                         where el.Name == "extensions"
                                         select el).Any();
            if (extensionsInAncestors)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Our normal alignment, which just adds 2 extra spaces to value attribute widths.
        /// </summary>
        private readonly IAlignmentFinder simpleAlignmentWithExtraValueWidth =
            new SimpleAlignment(new Dictionary<string, int>{
                {"value", 2}
            });

        /// <summary>
        /// Our slightly sophisticated way of grouping attributes for alignment in extensions.
        /// </summary>
        private readonly IAlignmentFinder extensionEnumAlignment
            = new GroupedAttributeAlignment(new GroupChoice(new AttributeGroup("value"),
                                                            new AttributeGroup("offset", "dir", "extends"),
                                                            new AttributeGroup("bitpos", "extends")));

        /// <summary>
        /// This is the recursive part that contains most of the "policy"
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="element">The element to write</param>
        protected override void WriteElement(XmlWriter writer, XElement element)
        {
            // Setup work: Sort return values if desired.
            if (element.Name == "command" && SortReturnVals)
            {
                var success = element.Attribute("successcodes");
                if (success != null)
                {
                    success.Value = CodeSorter.SortReturnCodeString(success.Value);
                }

                var error = element.Attribute("errorcodes");
                if (error != null)
                {
                    error.Value = CodeSorter.SortReturnCodeString(error.Value);
                }
            }

            // Now, only one of these paths should execute.
            if (ChildrenShouldBeSingleLine(element))
            {
                WriteSingleLineElement(writer, element);
            }
            else if (element.Name == "require" && element.Parent != null && element.Parent.Name == "feature")
            {
                // Simple alignment in feature requirements (core)
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "require" && element.Parent != null && element.Parent.Name == "extension")
            {
                // Beautiful alignment of the enums in extensions.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          extensionEnumAlignment,
                                                          (XElement element) => element.Name == "enum");
            }
            else if (element.Name == "tags" && element.HasElements)
            {
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "enums" && element.HasElements)
            {
                // Give some extra width to the value field
                // and don't let comments break up our alignment groups.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          simpleAlignmentWithExtraValueWidth,
                                                          IsEnum,
                                                          n => XmlUtilities.IsWhitespaceOrCommentBetweenSelectedElements(n, IsEnum));
            }
            else if (element.Name == "types")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, element, IsBitmask);
            }
            else if (element.Name == "interaction_profile")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, element, (XElement element) => element.Name == "component");
            }
            else if (WrapExtensions && element.Name == "extension")
            {
                // This will change the format! (for the better, probably, though)
                // Also, missing indent at level "extensions" so we adjust by -1
                WriteElementWithAttrNewlines(writer, element, -1);
            }
            else
            {
                base.WriteElement(writer, element);
            }

        }

        /// <inheritdoc />
        protected override XText CleanWhitespaceNode(XText whitespaceText)
            => FormatterUtilities.RegenerateIndentation(this, whitespaceText);
    }
}

