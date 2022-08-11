// Copyright 2021-2022 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.IO;
using Xunit;


namespace PrettyRegistryXml.OpenXR.Tests
{
    public class PreserveSchemasTest
    {
        public static IEnumerable<object[]> RoundTripFilenames => new List<object[]>{
            new object[] {
                "sample.xml"
            },
        };
        [Theory]
        [MemberData(nameof(RoundTripFilenames))]
        public void RoundtripData(string filename)
        {
            var fullPath = Path.Join(TestDataUtils.GetTestFileDir(), filename);
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
            Assert.Equal(TestDataUtils.NormalizeText(orig), TestDataUtils.NormalizeText(formatted));
        }

    }
}
