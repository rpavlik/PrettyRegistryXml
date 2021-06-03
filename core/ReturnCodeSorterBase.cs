// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Base utility class for sorting return codes, with some provided codes always in a given order,
    /// and the rest alphabetical after that.
    /// </summary>
    public class ReturnCodeSorterBase
    {
        private readonly Dictionary<string, Tuple<string, int>> importance;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="presortedSpecialItems">Your "special" codes, in the order you want them to appear.</param>
        public ReturnCodeSorterBase(string[] presortedSpecialItems)
            => importance = presortedSpecialItems
                                // reverse so that later items get a smaller index
                                .Reverse()
                                // turn items into an item, increased-reverse-index tuple
                                .Select((item, index) => (item, index + 100).ToTuple())
                                .ToDictionary(keySelector: tup => tup.Item1, elementSelector: tup => tup);


        /// <summary>
        /// Sorts a string of comma-separated return codes.
        /// </summary>
        /// <remarks>
        /// A few common, generic codes are sorted explicitly to the front of the list,
        /// with all remaining codes alphabetical after them.
        /// </remarks>
        /// <param name="vals">comma-separated return codes</param>
        /// <returns>comma-separated return codes in the desired order</returns>
        public string SortReturnCodeString(string vals)
            => string.Join(',', vals.Split(',', StringSplitOptions.RemoveEmptyEntries).OrderByDescending(GetKey));
        private Tuple<string, int> GetKey(string item)
        {
            if (importance.TryGetValue(item, out var ret))
            {
                return ret;
            }
            return (item, 0).ToTuple();
        }
    }
}
