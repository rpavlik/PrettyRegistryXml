// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PrettyRegistryXml.Vulkan
{
    public class Options
    {
        // Path to original vk.xml file from Vulkan
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

        // Whether to align attributes of children of <spirvextension> and <spirvcapability> tags.
        public bool AlignSPIRV { get; set; } = false;

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
                        case "align-spir-v":
                            options.AlignSPIRV = val;
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
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "- Align attributes of children of SPIR-V tags: {0}", AlignSPIRV));
            return sb.ToString();
        }
    }
}
