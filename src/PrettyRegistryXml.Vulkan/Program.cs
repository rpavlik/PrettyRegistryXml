// Copyright 2021-2022 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

using CommandLine;
using PrettyRegistryXml.Core;
using System;
using System.IO;
using System.Xml.Linq;

namespace PrettyRegistryXml.Vulkan
{

    sealed class Program
    {

        static void Run(Options options)
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
            new Parser(with =>
             {
                 with.GetoptMode = true;
             }).ParseArguments<Options>(args)
                          .WithParsed(Run);
        }
    }
}
