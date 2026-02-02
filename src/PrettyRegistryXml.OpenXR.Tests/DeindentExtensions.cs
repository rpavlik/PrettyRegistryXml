// Copyright 2021-2026 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.IO;
using Xunit;


namespace PrettyRegistryXml.OpenXR.Tests
{
    public class DeindentExtensionsTest
    {
        public static IEnumerable<TheoryDataRow<string, bool>> RoundTripFilenamesAndDeindentSettings => [
            new TheoryDataRow<string,bool>("deindent-off.xml", false),
            new TheoryDataRow<string,bool>("deindent-on.xml", true),
        ];

        [Theory]
        [MemberData(nameof(RoundTripFilenamesAndDeindentSettings))]
        public void DeindentSettingRoundtrip(string filename, bool deindentSetting)
        {
            var fullPath = Path.Join(TestDataUtils.GetTestFileDir(), filename);
            var outFullPath = fullPath + ".out";

            var opts = new Options
            {
                InputFile = fullPath,
                OutputFile = outFullPath,
                WrapExtensions = false,
                SortCodes = false,
                DeindentExtensions = deindentSetting,
            };
            Program.Run(opts);
            var orig = File.ReadAllText(fullPath);
            var formatted = File.ReadAllText(outFullPath);
            Assert.Equal(TestDataUtils.NormalizeText(orig), TestDataUtils.NormalizeText(formatted));
        }

    }
}
