// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Xml;
using PrettyRegistryXml.Core;

namespace PrettyRegistryXml.GroupedAlignment.Tests
{
    public class GroupedAlignmentTest
    {
        private static GroupedAttributeAlignment OpenXREnumAlignment =>
                        new GroupedAttributeAlignment(new GroupChoice(new AttributeGroup("value"),
                                                                      new AttributeGroup("offset", "dir", "extends")));
        public static object[] ExtensionEnums => new object[]{
            // OpenXR data that got goofed up
            new object[]{
                new XElement("require",
                    new XElement("enum",
                                new XAttribute("value", "1"),
                                new XAttribute("name", "XR_EXT_view_configuration_depth_range_SPEC_VERSION")),
                    new XElement("enum",
                                new XAttribute("value", "\"XR_EXT_view_configuration_depth_range\""),
                                new XAttribute("name", "XR_EXT_VIEW_CONFIGURATION_DEPTH_RANGE_EXTENSION_NAME")),
                    new XElement("enum",
                                new XAttribute("offset", "0"),
                                new XAttribute("extends", "XrStructureType"),
                                new XAttribute("name", "XR_TYPE_VIEW_CONFIGURATION_DEPTH_RANGE_EXT"))),
                OpenXREnumAlignment,
                new string[]{"value", "name"},
            },

        };

        /// <summary>
        /// Test a provided config, aligning varied attributes,
        /// and check that the specified attributes, if present, are in the same position in each line.
        /// </summary>
        /// <param name="requireElement">The parent "require" element</param>
        /// <param name="alignment">The GroupedAttributeAlignment to format with.</param>
        /// <param name="alignedAttrs">Attribute names to check for alignment</param>
        [MemberData(nameof(ExtensionEnums))]
        [Theory]
        public void EnumsAlignNames(XElement requireElement, GroupedAttributeAlignment alignment, string[] alignedAttrs)
        {
            var alignState = alignment.FindAlignment(requireElement.Elements());
            var lines = new List<string>();
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                foreach (XElement child in requireElement.Elements())
                {
                    XmlFormatterBase.WriteUsingWrappedWriter(writer, (innerWriter, innerSb) =>
                    {
                        innerWriter.WriteStartElement(child.Name.LocalName);
                        XmlFormatterBase.WriteAlignedAttrs(innerWriter, child, alignState, innerSb);
                        innerWriter.WriteEndElement();
                    });
                    writer.Flush();
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
            }

            foreach (var attrName in alignedAttrs)
            {
                var q = from line in lines
                        let pos = line.IndexOf(attrName)
                        // skip lines without this attribute.
                        where pos != -1
                        select pos;
                var uniqueColumns = q.Distinct().ToArray();
                Assert.Single(uniqueColumns);
            }

        }

    }
}
