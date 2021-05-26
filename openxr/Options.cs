// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;

namespace PrettyRegistryXml.OpenXR
{
    public record Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original xr.xml file from OpenXR")]
        public string InputFile { get; init; }

        [Value(1, MetaName = "outputFile", Required = false, HelpText = "Path to write formatted output file. Defaults to the same as the input file.")]
        public string OutputFile
        {
            get => _outputFile == null ? InputFile : _outputFile;
            init => _outputFile = value;
        }

        private string _outputFile;

        [Option("wrap-extensions", Default = (bool)false, HelpText = "Whether to wrap attributes of <extension> tags.")]
        public bool WrapExtensions { get; init; }
    }
}
