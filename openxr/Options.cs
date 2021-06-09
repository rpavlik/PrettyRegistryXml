// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace PrettyRegistryXml.OpenXR
{
    public record Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original xr.xml file from OpenXR")]
        public string InputFile { get; init; }

        [Value(1, MetaName = "outputFile", HelpText = "Path to write formatted output file. Defaults to the same as the input file.")]
        public string OutputFile { get; init; }

        /// <summary>
        /// This will be <see cref="Options.OutputFile">, if set, otherwise <see cref="Options.InputFile">
        /// </summary>
        /// <value></value>
        public string ActualOutputFile
        {
            get => OutputFile == null ? InputFile : OutputFile;
        }

        [Option("wrap-extensions", Default = (bool)false, HelpText = "Whether to wrap attributes of <extension> tags.")]
        public bool WrapExtensions { get; init; }

        [Option("sort-codes", Default = (bool)false, HelpText = "Whether to sort success and error codes.")]
        public bool SortCodes { get; init; }

        [Option("no-align-indent", Default = (bool)true, HelpText = "Whether to adjust indentation and align attributes where specified by policy.")]
        public bool AlignAndIndent { get; init; }

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
    }
}
