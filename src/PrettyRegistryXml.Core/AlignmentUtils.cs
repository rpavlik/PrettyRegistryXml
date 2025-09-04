
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PrettyRegistryXml.Core
{
    public class AlignmentUtils
    {

        private struct Entry
        {
            uint row;
            uint col;
            uint width;
        }
        public void StartNewRow() { nextCol = 0; currentRow++; }

        public void PushWidthForNextColumn(int size)
        {
            PadTo(row: currentRow, col: nextCol);
            SetWidthForRowColumn(row: currentRow, col: nextCol, width: size);
            nextCol++;
        }

        private void PadTo(int row, int col)
        {
            while (columnWidths.Count <= col)
            {
                columnWidths.Add(new List<int>());
            }
            List<int> column = columnWidths[col];
            while (column.Count < row)
            {
                column.Add(0);
            }
        }
        private void SetWidthForRowColumn(int row, int col, int width)
        {
            columnWidths[col][row] = width;
        }

        private int nextCol;
        private int currentRow;

        private List<List<int>> columnWidths = new();
    }


}
