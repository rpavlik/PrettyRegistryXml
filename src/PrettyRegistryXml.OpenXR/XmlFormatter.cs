// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using PrettyRegistryXml.Core;
using PrettyRegistryXml.GroupedAlignment;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System;


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

        private readonly Predicate<XElement> childrenShouldBeSingleLine;

        private bool WrapExtensions { get; init; }
        private bool WrapCommands { get; init; }

        private ReturnCodeSorter CodeSorter = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Formatting options, typically from command line.</param>
        public XmlFormatter(Options options)
        {
            WrapExtensions = options.WrapExtensions;
            WrapCommands = options.WrapCommands;

            var singleLineContainers = new HashSet<string> { "member", "param", "proto" };

            Predicate<XElement> isCategoryNonSingleLine = element =>
            {
                var category = element.Attribute("category");
                return category != null
                       && (category.Value == "define" || category.Value == "struct");
            };

            childrenShouldBeSingleLine = e =>
            {
                // some containers should always have their children on a single line.
                if (singleLineContainers.Contains(e.Name.LocalName)) return true;

                // some elements should only have their children on a single line when they're a specific usage:
                // lots of elts named "type" but they aren't all the same.
                return (e.Name.LocalName == "type"
                        && e.Parent != null
                        && e.Parent.Name.LocalName == "types"
                        && !isCategoryNonSingleLine(e));
            };
        }

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

        private IAlignmentFinder simpleAlignmentWithExtraValueWidth =
            new SimpleAlignment(new Dictionary<string, int>{
                {"value", 2}
            });

        private IAlignmentFinder extensionEnumAlignment
            = new GroupedAttributeAlignment(new GroupChoice(new AttributeGroup("value"),
                                                            new AttributeGroup("offset", "dir", "extends"),
                                                            new AttributeGroup("bitpos", "extends")));

        /// <summary>
        /// This is the recursive part that contains most of the "policy"
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="e">The element to write</param>
        protected override void WriteElement(XmlWriter writer, XElement e)
        {
            if (e.Name == "command")
            {
                var success = e.Attribute("successcodes");
                if (success != null)
                {
                    success.Value = CodeSorter.SortReturnCodeString(success.Value);
                }

                var error = e.Attribute("errorcodes");
                if (error != null)
                {
                    error.Value = CodeSorter.SortReturnCodeString(error.Value);
                }
            }

            if (childrenShouldBeSingleLine(e))
            {
                WriteSingleLineElement(writer, e);
            }
            else if (e.Name == "require" && e.Parent != null && e.Parent.Name == "feature")
            {
                // Simple alignment in feature requirements (core)
                WriteElementWithAlignedChildElts(writer, e);
            }
            else if (e.Name == "require" && e.Parent != null && e.Parent.Name == "extension")
            {
                // Beautiful alignment of the enums in extensions.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          e,
                                                          extensionEnumAlignment,
                                                          (XElement element) => element.Name == "enum");
            }
            else if (e.Name == "tags" && e.HasElements)
            {
                WriteElementWithAlignedChildElts(writer, e);
            }
            else if (e.Name == "enums" && e.HasElements)
            {
                // Give some extra width to the value field
                // and don't let comments break up our alignment groups.
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          e,
                                                          simpleAlignmentWithExtraValueWidth,
                                                          IsEnum,
                                                          n => XmlUtilities.IsWhitespaceOrCommentBetweenSelectedElements(n, IsEnum));
            }
            else if (e.Name == "types")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, e, IsBitmask);
            }
            else if (e.Name == "interaction_profile")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, e, (XElement element) => element.Name == "component");
            }
            else if (WrapCommands && e.Name == "command" && e.Parent != null && e.Parent.Name == "commands")
            {
                // This will change the format! (for the better, probably, though)
                WriteElementWithAttrNewlines(writer, e);
            }
            else if (WrapExtensions && e.Name == "extension")
            {
                // This will change the format! (for the better, probably, though)
                // Also, missing indent at level "extensions" so we adjust by -1
                WriteElementWithAttrNewlines(writer, e, -1);
            }
            else
            {
                base.WriteElement(writer, e);
            }

        }

        /// <inheritdoc />
        protected override XText CleanWhitespaceNode(XText whitespaceText)
            => FormatterUtilities.RegenerateIndentation(this, whitespaceText);
    }
}

