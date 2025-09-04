// Copyright 2025 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// The location (row and column) and width of a "cell", for aligning.
    /// </summary>
    public struct RowColWidth
    {
        /// <summary>
        /// Zero-indexed row number
        /// </summary>
        public uint row { get; init; }

        /// <summary>
        /// Zero-indexed column number
        /// </summary>
        public uint col { get; init; }

        /// <summary>
        /// Width of the cell
        /// </summary>
        public uint width { get; init; }

        /// <summary>
        /// Given a collection of <c>RowColumnWidth</c> entries, find the max column width of each column.
        /// </summary>
        /// <param name="entries">A collection of <c>RowColWidth</c> data representing an alignment problem </param>
        /// <returns>The max column width, for every column from 0 to the largest column number in the input, inclusive.</returns>
        public static IEnumerable<uint> FindColumnWidths(IEnumerable<RowColWidth> entries)
        {
            var maxRow = entries.MaxBy(e => e.row).row;
            var maxCol = entries.MaxBy(e => e.col).col;

            uint[] widths = new uint[maxCol + 1];

            for (uint col = 0; col <= maxCol; ++col)
            {
                uint width = (from e in entries
                              let w = e.col == col ? e.width : 0
                              select w).Max();
                widths[col] = width;
            }
            return widths;
        }

    }
}
