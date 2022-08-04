// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PrettyRegistryXml.Vulkan
{

    class Program
    {

        static void Run(Options options)
        {
            Console.WriteLine("Configuration:");
            Console.WriteLine(options.ToString());
            Console.WriteLine($"Reading registry from {options.InputFile}");
            XDocument document;
            using (var reader = new StreamReader(options.InputFile))
            {
                document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }

            Console.WriteLine("Processing with formatter");
            var formatter = new XmlFormatter(options);
            var result = formatter.Process(document);

            Console.WriteLine($"Writing processed registry to {options.ActualOutputFile}");
            using var writer = new StreamWriter(options.ActualOutputFile, false, Encoding.UTF8);
            writer.WriteLine(result);
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                          .WithParsed(Run);
        }
    }
}
