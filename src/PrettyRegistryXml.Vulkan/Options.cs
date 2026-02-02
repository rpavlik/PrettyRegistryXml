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
    public record Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original vk.xml file from Vulkan")]
        public string InputFile { get; init; }

        [Value(1, MetaName = "outputFile", HelpText = "Path to write formatted output file. Defaults to the same as the input file.")]
        public string OutputFile { get; init; }

        /// <summary>
        /// This will be <see cref="OutputFile"/>, if set, otherwise <see cref="InputFile"/>
        /// </summary>
        /// <value></value>
        public string ActualOutputFile
        {
            get => OutputFile ?? InputFile;
        }

        [Option("wrap-extensions", Default = false, HelpText = "Whether to wrap attributes of <extension> tags.")]
        public bool WrapExtensions { get; init; }

        [Option("align-spir-v", Default = false, HelpText = "Whether to align attributes of children of <spirvextension> and <spirvcapability> tags.")]
        public bool AlignSPIRV { get; init; }

        // Automatically used by CommandLineParser for help.

        [Usage(ApplicationAlias = "PrettyRegistryXml.Vulkan")]
        public static IEnumerable<Example> Examples
        {
            get => new List<Example>()
                {
                    new("Format the registry file in-place",
                                new Options { InputFile = "../vulkan/xml/vk.xml" }),
                    new("Format the registry file, with experimental extension attribute wrapping, to a new file",
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
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Input file: {InputFile}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Output file: {ActualOutputFile}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Wrap extensions attributes: {WrapExtensions}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Align attributes of children of SPIR-V tags: {AlignSPIRV}");
            return sb.ToString();
        }
    }
}
