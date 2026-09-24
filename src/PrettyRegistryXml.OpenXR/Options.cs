// Copyright 2021-2023 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PrettyRegistryXml.OpenXR
{
    public class Options
    {
        // Path to original xr.xml file from OpenXR
        public string InputFile { get; set; }

        // Path to write formatted output file. Defaults to the same as the input file.
        public string OutputFile { get; set; }

        /// <summary>
        /// This will be <see cref="OutputFile"/>, if set, otherwise <see cref="InputFile"/>
        /// </summary>
        /// <value></value>
        public string ActualOutputFile
        {
            get { return OutputFile != null ? OutputFile : InputFile; }
        }

        // Whether to wrap attributes of <extension> tags.
        public bool WrapExtensions { get; set; } = false;

        // Whether to trim the values of attributes.
        public bool TrimAttributes { get; set; } = true;

        // Whether to normalize spaces in the values of attributes.
        public bool NormalizeAttributeSpaces { get; set; } = true;

        // Whether to sort success and error codes.
        public bool SortCodes { get; set; } = true;

        // Whether to artificially de-indent extensions by one level.
        public bool DeindentExtensions { get; set; } = true;

        public static Options Parse(string[] args)
        {
            var options = new Options();
            var posArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    string option = args[i].Substring(2);
                    string name;
                    bool val;
                    int eqIndex = option.IndexOf('=');
                    if (eqIndex >= 0)
                    {
                        name = option.Substring(0, eqIndex);
                        string valStr = option.Substring(eqIndex + 1);
                        bool.TryParse(valStr, out val);
                    }
                    else
                    {
                        name = option;
                        val = true;
                    }

                    switch (name)
                    {
                        case "wrap-extensions":
                            options.WrapExtensions = val;
                            break;
                        case "trim-attributes":
                            options.TrimAttributes = val;
                            break;
                        case "normalize-attribute-spaces":
                            options.NormalizeAttributeSpaces = val;
                            break;
                        case "sort-codes":
                            options.SortCodes = val;
                            break;
                        case "deindent-extensions":
                            options.DeindentExtensions = val;
                            break;
                        default:
                            Console.WriteLine("Ignoring unknown option {0}",
                                              args[i]);
                            break;
                    }
                }
                else
                {
                    posArgs.Add(args[i]);
                }
            }

            if (posArgs.Count > 2)
            {
                Console.WriteLine("Too many positional arguments: {0}",
                                  posArgs.Count);
            }
            else if (posArgs.Count == 2)
            {
                options.InputFile = posArgs[0];
                options.OutputFile = posArgs[1];
            }
            else if (posArgs.Count == 1)
            {
                options.InputFile = posArgs[0];
            }

            return options;
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
