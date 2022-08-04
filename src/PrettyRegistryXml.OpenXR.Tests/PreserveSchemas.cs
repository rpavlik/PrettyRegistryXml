// Copyright 2021-2022 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;


namespace PrettyRegistryXml.OpenXR.Tests
{
    public class PreserveSchemasTest
    {
        private static string GetTestFileDir()
        {
            var assemblyPath = Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath);
            return Path.Join(Path.GetDirectoryName(assemblyPath), "TestFiles");
        }
        private static string NormalizeText(string s)
        {
            return s.ReplaceLineEndings().Trim();
        }

        public static IEnumerable<object[]> RoundTripFilenames => new List<object[]>{
            new object[] {
                "sample.xml"
            },
        };
        [Theory]
        [MemberData(nameof(RoundTripFilenames))]
        public void RoundtripData(string filename)
        {
            var fullPath = Path.Join(GetTestFileDir(), filename);
            var outFullPath = fullPath + ".out";

            var opts = new Options
            {
                InputFile = fullPath,
                OutputFile = outFullPath,
                WrapExtensions = false,
                SortCodes = false,
            };
            Program.Run(opts);
            var orig = File.ReadAllText(fullPath);
            var formatted = File.ReadAllText(outFullPath);
            Assert.Equal(NormalizeText(orig), NormalizeText(formatted));
        }

    }
}
