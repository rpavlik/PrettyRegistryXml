// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Xml.Linq;
using System.Linq;
using System;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Assorted utilities factored out from standard-specific formatters.
    /// </summary>
    public static class FormatterUtilities
    {
        /// <summary>
        /// A useful implementation for <see cref="XmlFormatterBase.CleanWhitespaceNode(XText)"/>
        /// that replaces any whitespace-only text node that contains a newline, with an equivalent
        /// text node with the same number of newlines and the "correct" indentation.
        /// </summary>
        /// <param name="formatter">Your formatter</param>
        /// <param name="whitespaceText">A whitespace-only text node.</param>
        /// <returns>A whitespace-only text node - possibly the same, possibly new.</returns>
        public static XText RegenerateIndentation(XmlFormatterBase formatter, XText whitespaceText)
        {
            // Don't bother modifying a whitespace-only node without a newline: won't affect indent.
            if (!whitespaceText.Value.Contains("\n")) { return whitespaceText; }

            // Completely replace whitespace-only nodes that do contain a newline:
            // keep total number of newlines the same, but re-construct with correct indent.

            var cleanNewlines = string.Join(null, (from c in whitespaceText.Value
                                                   where c == '\n'
                                                   select Environment.NewLine));

            // this is a heuristic but seems to work.
            bool followedByClosingTag = whitespaceText.NextNode == null;

            // This seems to be the most robust way to get the indent right.
            XNode indentDeterminingNode = whitespaceText;
            if (followedByClosingTag && whitespaceText.Parent != null)
            {
                indentDeterminingNode = whitespaceText.Parent;
            }
            var indent = formatter.MakeIndent(indentDeterminingNode);

            return new XText(cleanNewlines + indent);
        }

        /// <summary>
        /// Generate strings of spaces of any width.
        /// </summary>
        /// <param name="width">A non-negative number</param>
        /// <returns>A string of size <paramref name="width"/> of only spaces</returns>
        public static string MakeSpaces(int width)
        {
            if (width < 0) {
                throw new ArgumentOutOfRangeException("Cannot make negative spaces");
            }
            if (width == 0) {
                return string.Empty;
            }
            return "".PadRight(width);
        }
    }
}
