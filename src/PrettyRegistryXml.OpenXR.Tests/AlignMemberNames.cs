// Copyright (c) Meta Platforms, LLC and its affiliates. All rights reserved.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.IO;
using Xunit;


namespace PrettyRegistryXml.OpenXR.Tests
{
    public class AlignMemberNamesTest
    {
        private static string Format(string filename, bool alignMemberNames)
        {
            var fullPath = Path.Join(TestDataUtils.GetTestFileDir(), filename);
            var outFullPath = fullPath + (alignMemberNames ? ".aligned.out" : ".unaligned.out");

            var opts = new Options
            {
                InputFile = fullPath,
                OutputFile = outFullPath,
                SortCodes = false,
                DeindentExtensions = true,
                AlignMemberNames = alignMemberNames,
            };
            Program.Run(opts);
            return TestDataUtils.NormalizeText(File.ReadAllText(outFullPath));
        }

        private static string Read(string filename)
            => TestDataUtils.NormalizeText(File.ReadAllText(Path.Join(TestDataUtils.GetTestFileDir(), filename)));

        [Fact]
        public void AlignsMemberNames()
        {
            Assert.Equal(Read("align-members-out.xml"), Format("align-members-in.xml", true));
        }

        [Fact]
        public void AlignedMemberNamesAreStable()
        {
            Assert.Equal(Read("align-members-out.xml"), Format("align-members-out.xml", true));
        }

        [Fact]
        public void LeavesMemberNamesWhenDisabled()
        {
            Assert.Equal(Read("align-members-in.xml"), Format("align-members-in.xml", false));
        }
    }
}
