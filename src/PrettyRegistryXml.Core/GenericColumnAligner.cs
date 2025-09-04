// Copyright 2025 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Accumulates a collection of row, column, width tuples,
    /// to later summarize them by providing the maximum width for each column.
    /// </summary>
    public class GenericColumnAligner
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericColumnAligner()
        {
        }

        /// <summary>
        /// Indicate that a new row is being worked on.
        /// (Increments the row and resets the column)
        /// </summary>
        public void StartNewRow() { nextCol = 0; currentRow++; }

        /// <summary>
        /// Append a column for the current row with the given width
        /// </summary>
        /// <param name="width">Width of this cell</param>
        public void PushWidthForNextColumn(uint width)
        {
            SetWidthForRowColumn(row: currentRow, col: nextCol, width: width);
            nextCol++;
        }

        private void SetWidthForRowColumn(uint row, uint col, uint width)
        {
            // columnWidths[col][row] = width;

            entries.RemoveAll(e => e.row == row && e.col == col);
            var e = new RowColWidth { row = row, col = col, width = width };
            entries.Add(e);
        }

        /// <summary>
        /// Find the max column width of each column.
        /// </summary>
        /// <returns>The max column width, for every column from 0 to the largest column number in the accumulated input, inclusive.</returns>
        public IEnumerable<uint> GetColumnWidths()
        {
            return RowColWidth.FindColumnWidths(entries);
        }

        private uint nextCol;
        private uint currentRow;

        private List<RowColWidth> entries = new();

    }


}
