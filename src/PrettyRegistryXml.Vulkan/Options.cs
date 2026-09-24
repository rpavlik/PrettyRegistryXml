// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Disabling in this file because there's help text for each, lack of XML docs doesn't both me here.
#pragma warning disable CS1591

namespace PrettyRegistryXml.Vulkan
{
    public class Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original vk.xml file from Vulkan")]
        public string InputFile { get; set; }

        [Value(1, MetaName = "outputFile", HelpText = "Path to write formatted output file. Defaults to the same as the input file.")]
        public string OutputFile { get; set; }

        /// <summary>
        /// This will be <see cref="OutputFile"/>, if set, otherwise <see cref="InputFile"/>
        /// </summary>
        /// <value></value>
        public string ActualOutputFile
        {
            get { return OutputFile != null ? OutputFile : InputFile; }
        }

        [Option("wrap-extensions", Default = false, HelpText = "Whether to wrap attributes of <extension> tags.")]
        public bool WrapExtensions { get; set; } = false;

        [Option("align-spir-v", Default = false, HelpText = "Whether to align attributes of children of <spirvextension> and <spirvcapability> tags.")]
        public bool AlignSPIRV { get; set; } = false;

        // Automatically used by CommandLineParser for help.

        [Usage(ApplicationAlias = "PrettyRegistryXml.Vulkan")]
        public static IEnumerable<Example> Examples
        {
            get {
              return new List<Example>()
                {
                    new Example("Format the registry file in-place",
                                new Options { InputFile = "../vulkan/xml/vk.xml" }),
                    new Example("Format the registry file, with experimental extension attribute wrapping, to a new file",
                                new Options {
                                    InputFile = "../vulkan/xml/vk.xml" ,
                                    OutputFile = "../vulkan/xml/vk-wrapped.xml",
                                    WrapExtensions = true,
                                }),
                };
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Input file: {0}", InputFile));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Output file: {0}", ActualOutputFile));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Wrap extensions attributes: {0}", WrapExtensions));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Align attributes of children of SPIR-V tags: {0}", AlignSPIRV));
            return sb.ToString();
        }
    }
}
