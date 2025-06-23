// Copyright 2025 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Text.RegularExpressions;

namespace PrettyRegistryXml.Core
{

    /// <summary>
    /// Utilities related to early processing of whitespace in text nodes.
    /// </summary>
    public static class TextWhitespace
    {

        /// <summary>
        /// The whitespace-related action to take upon encountering a text node
        /// </summary>
        public enum Behavior
        {
            /// Leave unmodified
            Preserve,
            /// Remove all leading whitespace
            TrimStart,
            /// Remove all trailing whitespace
            TrimEnd,
            /// Remove all leading and trailing whitespace
            Trim,
            /// Replace each contiguous run of whitespace with a single space
            Collapse,
            /// If the text isn't empty/null, it should have a single trailing space, and no leading space.
            EmptyOrSingleTrailingSpace,
            /// Remove all leading whitespace, then replace each remaining contiguous run of whitespace with a single space
            TrimStartAndCollapse,

        }

        /// <summary>
        /// Perform the specified whitespace transformation.
        /// </summary>
        /// <param name="behavior">The behavior to apply</param>
        /// <param name="value">the input value</param>
        /// <returns>the result of the transformation</returns>
        public static string ApplyBehavior(Behavior behavior, string value)
        {
            const string regex_pattern = @"\s+";
            switch (behavior)
            {
                case Behavior.Preserve:
                    return value;
                case Behavior.TrimStart:
                    return value.TrimStart();
                case Behavior.TrimEnd:
                    return value.TrimEnd();
                case Behavior.Trim:
                    return value.Trim();
                case Behavior.Collapse:
                    return Regex.Replace(value, regex_pattern, " ");
                case Behavior.EmptyOrSingleTrailingSpace:
                    var temp = value.Trim();
                    if (temp.Length == 0) { return temp; }
                    return temp + " ";
                case Behavior.TrimStartAndCollapse:
                    return Regex.Replace(value.TrimStart(), regex_pattern, " ");

            }
            return value;
        }

        /// <summary>
        /// Perform the specified whitespace transformation, treating empty strings like null and vice versa.
        /// </summary>
        /// <param name="behavior">The behavior to apply</param>
        /// <param name="value">the nullable input value</param>
        /// <returns>the result of the transformation, an empty string if input or result was null or empty</returns>
        public static string ApplyBehaviorNullable(Behavior behavior, string? value)
        {
            if (value == null) { return ""; }
            var result = ApplyBehavior(behavior, value);

            if (result == null)
            {
                return "";
            }
            return result;
        }
    }

}
