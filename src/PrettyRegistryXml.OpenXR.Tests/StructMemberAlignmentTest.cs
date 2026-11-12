// Copyright 2026 Google LLC
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Xml.Linq;
using Xunit;

namespace PrettyRegistryXml.OpenXR.Tests
{
    public class StructMemberAlignmentTest
    {
        private static string FormatXml(string xml, bool alignMemberNames)
        {
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            var options = new Options
            {
                AlignMemberNames = alignMemberNames
            };
            var formatter = new XmlFormatter(options);
            return TestDataUtils.NormalizeText(formatter.Process(doc));
        }

        [Fact]
        public void MemberWithStraySpaceBeforeType()
        {
            const string input = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<registry>
    <types>
        <type category=""struct"" name=""XrSwapchainImageD3D11KHR"">
            <member values=""XR_TYPE_SWAPCHAIN_IMAGE_D3D11_KHR""> <type>XrStructureType</type> <name>type</name></member>
            <member>  const <type>void</type>*      <name>next</name></member>
            <member> struct <type>ID3D11Texture2D</type>* <name>texture</name></member>
        </type>
    </types>
</registry>";

            const string expected = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<registry>
    <types>
        <type category=""struct"" name=""XrSwapchainImageD3D11KHR"">
            <member values=""XR_TYPE_SWAPCHAIN_IMAGE_D3D11_KHR""><type>XrStructureType</type> <name>type</name></member>
            <member>const <type>void</type>*      <name>next</name></member>
            <member>struct <type>ID3D11Texture2D</type>* <name>texture</name></member>
        </type>
    </types>
</registry>";

            var formatted = FormatXml(input, false);
            Assert.Equal(TestDataUtils.NormalizeText(expected), formatted);

            var expectedAligned = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<registry>
    <types>
        <type category=""struct"" name=""XrSwapchainImageD3D11KHR"">
            <member values=""XR_TYPE_SWAPCHAIN_IMAGE_D3D11_KHR""><type>XrStructureType</type> <name>type</name></member>
            <member>const <type>void</type>*             <name>next</name></member>
            <member>struct <type>ID3D11Texture2D</type>* <name>texture</name></member>
        </type>
    </types>
</registry>";

            var formattedAligned = FormatXml(input, true);
            Assert.Equal(TestDataUtils.NormalizeText(expectedAligned), formattedAligned);
        }
    }
}
