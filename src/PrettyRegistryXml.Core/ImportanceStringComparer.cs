// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

namespace PrettyRegistryXml.Core
{

    /// <summary>
    /// Implementation of <see cref="IComparer{T}"/> for pairs of integer importance and string.
    /// </summary>
    internal class ImportanceStringComparer : Comparer<Tuple<int, string>>
    {
        private readonly Comparer<int> intComparer = Comparer<int>.Default;
        private readonly StringComparer stringComparer = StringComparer.Ordinal;

        public override int Compare(Tuple<int, string>? x, Tuple<int, string>? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var intResult = intComparer.Compare(x.Item1, y.Item1);
            if (intResult != 0)
            {
                return intResult;
            }
            return stringComparer.Compare(x.Item2, y.Item2);
        }
    }
}
