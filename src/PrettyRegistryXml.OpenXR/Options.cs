// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Disabling in this file because there's help text for each, lack of XML docs doesn't bother me here.
#pragma warning disable CS1591

namespace PrettyRegistryXml.OpenXR
{
    public record Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original xr.xml file from OpenXR")]
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

        [Option("sort-codes", Default = true, HelpText = "Whether to sort success and error codes.")]
        public bool SortCodes { get; init; }

        // Automatically used by CommandLineParser for help.

        [Usage(ApplicationAlias = "PrettyRegistryXml.OpenXR")]
        public static IEnumerable<Example> Examples
        {
            get => new List<Example>()
                {
                    new Example("Format the registry file in-place",
                                new Options { InputFile = "../openxr/specification/registry/xr.xml" }),
                    new Example("Format the registry file, with experimental extension attribute wrapping, to a new file",
                                new Options {
                                    InputFile = "../openxr/specification/registry/xr.xml" ,
                                    OutputFile = "../openxr/specification/registry/xr-wrapped.xml",
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
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Sort return codes: {SortCodes}");
            return sb.ToString();
        }
    }
}
