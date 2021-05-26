// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: BSL-1.0

using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CommandLine;

namespace PrettyRegistryXml.OpenXR
{

    class Program
    {

        static void Run(Options options)
        {
            Console.WriteLine($"Reading registry from {options.InputFile}");
            XDocument document;
            using (var reader = new StreamReader(options.InputFile))
            {
                document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }

            Console.WriteLine("Processing with formatter");
            var formatter = new XmlFormatter(options);
            var result = formatter.Process(document);

            Console.WriteLine($"Writing processed registry to {options.OutputFile}");
            using (var writer = new StreamWriter(options.OutputFile, false, Encoding.UTF8))
            {
                writer.WriteLine(result);
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                          .WithParsed(Run);
        }
    }
}
