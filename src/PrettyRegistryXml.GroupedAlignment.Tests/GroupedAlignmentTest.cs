// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

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
                        new(new GroupChoice(new AttributeGroup("value"),
                                            new AttributeGroup("offset", "dir", "extends")));
        private static GroupedAttributeAlignment VulkanEnumAlignment =>
                        new(
                            new GroupChoice(new AttributeGroup("value"),
                                            new AttributeGroup("extends", "extnumber", "offset", "dir"),
                                            new AttributeGroup("bitpos", "extends")),
                            new AlignedTrailer());
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
            // Vulkan data that triggered a crash
            new object[]{
                new XElement("require",
                    new XElement("enum",
                                new XAttribute("extends", "VkStructureType"),
                                new XAttribute("extnumber", "61"),
                                new XAttribute("offset", "11"),
                                new XAttribute("name", "VK_STRUCTURE_TYPE_DEVICE_GROUP_PRESENT_INFO_KHR")),
                    new XElement("enum",
                                new XAttribute("extends", "VkStructureType"),
                                new XAttribute("extnumber", "61"),
                                new XAttribute("offset", "12"),
                                new XAttribute("name", "VK_STRUCTURE_TYPE_DEVICE_GROUP_SWAPCHAIN_CREATE_INFO_KHR")),
                    new XElement("enum",
                                new XAttribute("bitpos", "0"),
                                new XAttribute("extends", "VkSwapchainCreateFlagBitsKHR"),
                                new XAttribute("name", "VK_SWAPCHAIN_CREATE_SPLIT_INSTANCE_BIND_REGIONS_BIT_KHR"),
                                new XAttribute("comment", "Allow images with VK_IMAGE_CREATE_SPLIT_INSTANCE_BIND_REGIONS_BIT"))),
                VulkanEnumAlignment,
                new string[]{"offset", "name"},
            },
            // Complicated mix of attributes in Vulkan data
            new object[]{
                new XElement("require",
                    new XElement("enum",
                                new XAttribute("offset", "4"),
                                new XAttribute("extends", "VkStructureType"),
                                new XAttribute("extnumber", "162"),
                                new XAttribute("name", "VK_STRUCTURE_TYPE_DESCRIPTOR_SET_VARIABLE_DESCRIPTOR_COUNT_LAYOUT_SUPPORT")),
                    new XElement("enum",
                                new XAttribute("bitpos", "1"),
                                new XAttribute("extends", "VkDescriptorPoolCreateFlagBits"),
                                new XAttribute("name", "VK_DESCRIPTOR_POOL_CREATE_UPDATE_AFTER_BIND_BIT")),
                    new XElement("enum",
                                new XAttribute("offset", "0"),
                                new XAttribute("dir", "-"),
                                new XAttribute("extends", "VkResult"),
                                new XAttribute("extnumber", "162"),
                                new XAttribute("name", "VK_ERROR_FRAGMENTATION"))),
                VulkanEnumAlignment,
                new string[]{"offset", "name"},
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
            var lines = ToAlignedLines(requireElement, alignment);

            foreach (var attrName in alignedAttrs)
            {
                var q = from line in lines
                        let pos = line.IndexOf(attrName, System.StringComparison.InvariantCulture)
                        // skip lines without this attribute.
                        where pos != -1
                        select pos;
                var uniqueColumns = q.Distinct().ToArray();
                Assert.Single(uniqueColumns);
            }

        }

        public static object[] PreviousFailures => new object[]{
        // Vulkan data that triggered a "duplicate attribute"
            new object[]{
                new XElement("require",
                    new XElement("enum",
                                new XAttribute("value", "3"),
                                new XAttribute("name", "VK_KHR_SAMPLER_MIRROR_CLAMP_TO_EDGE_SPEC_VERSION")),
                    new XElement("enum",
                                new XAttribute("value", "\"VK_KHR_sampler_mirror_clamp_to_edge\""),
                                new XAttribute("name", "VK_KHR_SAMPLER_MIRROR_CLAMP_TO_EDGE_EXTENSION_NAME")),
                    new XElement("enum",
                                new XAttribute("value", "4"),
                                new XAttribute("extends", "VkSamplerAddressMode"),
                                new XAttribute("name", "VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE"),
                                new XAttribute("comment", "Note that this defines what was previously a core enum, and so uses the 'value' attribute rather than 'offset', and does not have a suffix. This is a special case, and should not be repeated")),
                    new XElement("enum",
                                new XAttribute("extends", "VkSamplerAddressMode"),
                                new XAttribute("name", "VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE_KHR"),
                                new XAttribute("alias", "VK_SAMPLER_ADDRESS_MODE_MIRROR_CLAMP_TO_EDGE"),
                                new XAttribute("comment", "Alias introduced for consistency with extension suffixing rules"))),
                OpenXREnumAlignment,
            },
        };

        /// <summary>
        /// Use a provided config to align varied attributes, without any particular assertions about the output.
        /// </summary>
        /// <param name="requireElement">The parent "require" element</param>
        /// <param name="alignment">The GroupedAttributeAlignment to format with.</param>
        [MemberData(nameof(PreviousFailures))]
        [Theory]
        public void CheckPreviousFailures(XElement requireElement, GroupedAttributeAlignment alignment)
        {
            var lines = ToAlignedLines(requireElement, alignment);
            Assert.Equal(requireElement.Elements().Count(), lines.Count());
        }

        /// <summary>
        /// Use a provided config to align varied attributes.
        /// </summary>
        /// <param name="requireElement">The parent "require" element</param>
        /// <param name="alignment">The GroupedAttributeAlignment to format with.</param>
        /// <returns>A collection of lines, each one with an element on it.</return>
        public static IEnumerable<string> ToAlignedLines(XElement requireElement, GroupedAttributeAlignment alignment)
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
            return lines;
        }

    }
}
