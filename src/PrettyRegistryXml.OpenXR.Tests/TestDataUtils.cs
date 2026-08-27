// Copyright 2021-2022 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.IO;
using System.Reflection;


namespace PrettyRegistryXml.OpenXR.Tests
{
    public class TestDataUtils
    {

        /// <summary>
        /// Computes the TestFiles directory at runtime.
        /// </summary>
        /// <returns>path to TestFiles</returns>
        public static string GetTestFileDir()
        {
            var assemblyPath = Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath);
            return Path.Join(Path.GetDirectoryName(assemblyPath), "TestFiles");
        }

        /// <summary>
        /// Normalizes line endings and trims trailing whitespace,
        /// for better comparisons of input to output.
        /// </summary>
        /// <param name="s">string</param>
        /// <returns>normalized string</returns>
        public static string NormalizeText(string s)
        {
            return s.ReplaceLineEndings().Trim();
        }

    }
}
