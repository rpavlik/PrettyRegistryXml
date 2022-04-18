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


namespace PrettyRegistryXml.Vulkan
{
    /// <summary>
    /// Vulkan-specific policy for formatting XML.
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
        /// Whether we should align the attributes of SPIR-V-related tags, a runtime preference set by the command line.
        /// </summary>
        private bool AlignSPIRV { get; init; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Formatting options, typically from command line.</param>
        public XmlFormatter(Options options)
        {
            WrapExtensions = options.WrapExtensions;
            AlignSPIRV = options.AlignSPIRV;
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
            if (singleLineContainers.Contains(e.Name.LocalName)) return true;

            // some elements should only have their children on a single line when they're a specific usage:
            // lots of elts named "type" but they aren't all the same.
            return e.Name.LocalName == "type"
                   && e.Parent != null
                   && e.Parent.Name.LocalName == "types"
                   && !IsCategoryNonSingleLine(e);
        }

        private static bool IsNodeIndentableComment(XNode node)
        {
            // Some comments get indented a bit extra
            if (node is XElement element && element.Name == "comment")
            {
                if (node.Parent == null)
                {
                    // top level gets extra indent
                    return true;
                }
                // also, any comment *not* under one of these elements
                // This is just trying to minimize the diff.
                var parentName = node.Parent.Name.ToString();
                return parentName is not "registry" and not "enums" and not "require";
            }
            return false;

        }

        private static bool IsEnum(XElement element) => element.Name == "enum";

        /// <inheritdoc />
        public override int ComputeLevelAdjust(XNode node)
        {
            // Some comments get indented a bit extra
            if (IsNodeIndentableComment(node))
            {
                return 1;
            }
            else if (node is XText text && string.IsNullOrWhiteSpace(text.Value))
            {
                if (node.NextNode != null)
                {
                    if (IsNodeIndentableComment(node.NextNode)) return 1;
                }
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
                                                            new AttributeGroup("extends", "extnumber", "offset", "dir"),
                                                            new AttributeGroup("bitpos", "extends")));

        /// <summary>
        /// Our alignment for attributes in type elements.
        /// </summary>
        private readonly IAlignmentFinder typeAlignment
            = new GroupedAttributeAlignment(new GroupChoice(new AttributeGroup("requires"),
                                                            new AttributeGroup("bitvalues"),
                                                            new AttributeGroup("name", "alias"),
                                                            new AttributeGroup("category", "parent", "objtypeenum")),
                                            new AttributeGroup("category"),
                                            new UnalignedTrailer());

        /// <summary>
        /// This is the recursive part that contains most of the "policy"
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="element">The element to write</param>
        protected override void WriteElement(XmlWriter writer, XElement element)
        {
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
                                                          alignmentFinder: extensionEnumAlignment,
                                                          groupingPredicate: (XElement element) => element.Name == "enum");
            }
            else if (element.Name == "tags" && element.HasElements)
            {
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "platforms" && element.HasElements)
            {
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (element.Name == "enums" && element.HasElements)
            {
                // Give some extra width to the value field
                WriteElementWithAlignedChildAttrsInGroups(writer,
                                                          element,
                                                          // Previous used this instead of simpleAlignmentWithExtraValueWidth, don't remember why
                                                          // maybe because of VkDebugReportObjectTypeEXT ?
                                                          // alignmentFinder: enumAlignment,
                                                          alignmentFinder: simpleAlignmentWithExtraValueWidth,
                                                          groupingPredicate: IsEnum,
                                                          // Don't let comments break up our alignment groups.
                                                          ignoreNodePredicate: n => XmlUtilities.IsWhitespaceOrCommentBetweenSelectedElements(n, IsEnum));
            }
            else if (element.Name == "types")
            {
                WriteElementWithAlignedChildAttrsInMultipleGroups(writer,
                                                                  element,
                                                                  alignmentFinder: typeAlignment,
                                                                  alignmentPredicate: (XElement element) =>
                                                                  {
                                                                      if (element.Name == "type")
                                                                      {
                                                                          var cat = element.Attribute("category");
                                                                          if (cat == null) return false;
                                                                          var catName = cat.Value;
                                                                          if (catName == "struct" && element.Attribute("alias") is not null)
                                                                          {
                                                                              // We can align these.
                                                                              return true;
                                                                          }
                                                                          // These categories look weird when aligned, so don't align them.
                                                                          return catName is not "define"
                                                                                 and not "funcpointer"
                                                                                 and not "struct"
                                                                                 and not "union"
                                                                                 and not "include";
                                                                      }
                                                                      return false;
                                                                  },
                                                                  groupingFunc: element => element.Attribute("category")?.Value);
            }
            else if (AlignSPIRV && element.Name == "spirvextension" && element.HasElements)
            {
                // Having this in here does change things, but for the better.
                WriteElementWithAlignedChildElts(writer, element);
            }
            else if (AlignSPIRV && element.Name == "spirvcapability" && element.HasElements)
            {
                // Having this in here does change things, but for the better.
                WriteElementWithAlignedChildElts(writer, element);
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

