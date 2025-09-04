// Copyright 2021-2023 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using CommandLine.Text;
using PrettyRegistryXml.Core;
using System;
using System.IO;
using System.Xml.Linq;

namespace PrettyRegistryXml.OpenXR
{
    /// <summary>
    /// The main entry point class for the OpenXR registry formatter
    /// </summary>
    public class Program
    {

        /// <summary>
        /// The inner method used to do the formatting once we have the options parsed.
        /// </summary>
        /// <param name="options"></param>
        public static void Run(Options options)
        {
            Console.WriteLine("Configuration:");
            Console.WriteLine(options.ToString());
            Console.WriteLine($"Reading registry from {options.InputFile}");

            var roundtripper = XmlRoundtripper.ParseAndLoad(options.InputFile, out XDocument document);

            Console.WriteLine("Processing with formatter");
            var formatter = new XmlFormatter(options);
            var result = formatter.Process(document);

            Console.WriteLine($"Writing processed registry to {options.ActualOutputFile}");
            using var outStream = new FileStream(options.ActualOutputFile, FileMode.Create, FileAccess.Write);
            roundtripper.Write(result, outStream);
        }

        static void Main(string[] args)
        {
            Parser parser = new Parser(with =>
            {
                with.GetoptMode = true;
            });
            ParserResult<Options> result = parser.ParseArguments<Options>(args);
            if (result.Tag == ParserResultType.NotParsed)
            {
                Console.WriteLine(HelpText.AutoBuild(result));
                return;
            }
            result.WithParsed(Run);
        }
    }
}
