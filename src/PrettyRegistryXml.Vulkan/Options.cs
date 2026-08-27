// Copyright 2021-2026 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Text;

// Disabling in this file because there's help text for each, lack of XML docs doesn't both me here.
#pragma warning disable CS1591

namespace PrettyRegistryXml.Vulkan
{
    public struct Options
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
            get => OutputFile ?? InputFile;
        }

        [Option("wrap-extensions", Default = false, HelpText = "Whether to wrap attributes of <extension> tags.")]
        public bool WrapExtensions { get; set; }

        [Option("align-spir-v", Default = false, HelpText = "Whether to align attributes of children of <spirvextension> and <spirvcapability> tags.")]
        public bool AlignSPIRV { get; set; }

        // Automatically used by CommandLineParser for help.

        [Usage(ApplicationAlias = "PrettyRegistryXml.Vulkan")]
        public static IEnumerable<Example> Examples
        {
            get => new List<Example>()
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"- Input file: {InputFile}");
            sb.AppendLine($"- Output file: {ActualOutputFile}");
            sb.AppendLine($"- Wrap extensions attributes: {WrapExtensions}");
            sb.AppendLine($"- Align attributes of children of SPIR-V tags: {AlignSPIRV}");
            return sb.ToString();
        }
    }
}
