// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: BSL-1.0

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;

namespace pretty_registry
{
    public class XmlFormatter : XmlFormatterBase
    {
        private readonly Predicate<XElement> childrenShouldBeSingleLine;

        public override string IndentChars { get => "    "; }

        public XmlFormatter()
        {
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
            var attr = element.Attribute("category");
            return element.Name.LocalName == "type"
                   && attr != null
                   && attr.Value == "bitmask";
        };

        // This is the recursive part
        protected override void WriteElement(XmlWriter writer, XElement e)
        {
            if (childrenShouldBeSingleLine(e))
            {
                WriteSingleLineElement(writer, e);
            }
            else if (e.Name == "require" && e.Parent.Name == "feature")
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
            // else if (e.Name == "extension")
            // {
            //     // This will change the format! (for the better, probably, though)
            //     // Also, missing indent at level "extensions" so we adjust by -1
            //     WriteElementWithAttrNewlines(writer, e, -1);
            // }
            else
            {
                base.WriteElement(writer, e);
            }

        }
    }
}
