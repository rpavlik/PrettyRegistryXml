// Copyright 2021-2023 Collabora, Ltd
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
    public class Options
    {
        [Value(0, MetaName = "inputFile", Required = true, HelpText = "Path to original xr.xml file from OpenXR")]
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

        [Option("trim-attributes", Default = true, HelpText = "Whether to trim the values of attributes.")]
        public bool TrimAttributes { get; set; } = true;

        [Option("normalize-attribute-spaces", Default = true, HelpText = "Whether to normalize spaces in the values of attributes.")]
        public bool NormalizeAttributeSpaces { get; set; } = true;

        [Option("sort-codes", Default = true, HelpText = "Whether to sort success and error codes.")]
        public bool SortCodes { get; set; } = true;

        [Option("deindent-extensions", Default = true, HelpText = "Whether to artificially de-indent extensions by one level.")]
        public bool DeindentExtensions { get; set; } = true;

        // Automatically used by CommandLineParser for help.

        [Usage(ApplicationAlias = "PrettyRegistryXml.OpenXR")]
        public static IEnumerable<Example> Examples
        {
            get {
                return new List<Example>()
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Input file: {0}", InputFile));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Output file: {0}", ActualOutputFile));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Wrap extensions attributes: {0}", WrapExtensions));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Trim attribute values: {0}", TrimAttributes));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Normalize spaces in attribute values: {0}", NormalizeAttributeSpaces));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- De-indent extensions: {0}", DeindentExtensions));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Sort return codes: {0}", SortCodes));
            return sb.ToString();
        }
    }
}
