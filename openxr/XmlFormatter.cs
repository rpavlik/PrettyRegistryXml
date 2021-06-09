// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using PrettyRegistryXml.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System;


namespace PrettyRegistryXml.OpenXR
{
    public class XmlFormatter : XmlFormatterBase
    {
        public override int IndentLevelWidth { get => 4; }
        public override string IndentChars { get => "    "; }

        private readonly Predicate<XElement> childrenShouldBeSingleLine;

        private bool WrapExtensions { get; init; }
        private bool SortReturnVals { get; init; }

        private ReturnCodeSorter CodeSorter = new();

        public XmlFormatter(Options options)
        {
            WrapExtensions = options.WrapExtensions;
            SortReturnVals = options.SortCodes;

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

        private System.Predicate<XNode> isBitmask = node =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return false;
            }
            var element = node as XElement;
            var attr = element?.Attribute("category");
            return element?.Name == "type"
                   && attr != null
                   && attr.Value == "bitmask";
        };

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
        // This is the recursive part
        protected override void WriteElement(XmlWriter writer, XElement e)
        {
            if (e.Name == "command" && SortReturnVals)
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
                WriteElementWithAlignedChildElts(writer, e);
            }
            else if (e.Name == "tags" && e.HasElements)
            {
                WriteElementWithAlignedChildAttrs(writer, e);
            }
            else if (e.Name == "enums" && e.HasElements)
            {
                // Give some extra width to the value field
                WriteElementWithAlignedChildAttrs(writer, e, new Dictionary<string, int>{
                    {"value", 2}
                });
            }
            else if (e.Name == "types")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, e, isBitmask);
            }
            else if (e.Name == "interaction_profile")
            {
                WriteElementWithAlignedChildAttrsInGroups(writer, e, node => node is XElement element && element.Name == "component");
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

        protected override XText CleanWhitespaceNode(XText whitespaceText)
            => FormatterUtilities.RegenerateIndentation(this, whitespaceText);
    }
}

