// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: BSL-1.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CommandLine;

namespace pretty_registry
{
    class Program
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
        }

        static void Run(Options options)
        {
            Console.WriteLine($"Reading registry from {options.InputFile}");
            XDocument document;
            using (var reader = new StreamReader(options.InputFile))
            {
                document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }

            Console.WriteLine("Processing with formatter");
            var formatter = new XmlFormatter();
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
