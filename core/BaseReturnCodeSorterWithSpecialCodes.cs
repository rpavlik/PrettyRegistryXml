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
    /// Base utility class for sorting return codes when some are "special",
    /// with some provided codes always in a given order,
    /// and the rest alphabetical after that.
    /// </summary>
    public abstract class BaseReturnCodeSorterWithSpecialCodes : BaseReturnCodeSorter
    {
        private readonly Dictionary<string, Tuple<int, string>> importance;

        /// <value>Your "special" codes, in the order you want them to appear.</value>
        /// <remarks>This is public for ease of unit testing</remarks>
        public abstract IEnumerable<string> PresortedSpecialCodes { get; }

        /// <summary>
        /// Constructor - processes your PresorterSpecialCodes
        /// </summary>
        public BaseReturnCodeSorterWithSpecialCodes()
        {
            importance = PresortedSpecialCodes
                                // reverse so that later items get a smaller index
                                .Reverse()
                                // turn items into an negated-decreased-reverse-index, item tuple
                                // .Select((item, index) => (-index - 100, item).ToTuple())
                                .Select((item, index) => (index + 100, item).ToTuple())
                                .ToDictionary(keySelector: tup => tup.Item2, elementSelector: tup => tup);
        }


        /// <summary>
        /// Sorts an enumerable of return codes.
        /// </summary>
        /// <remarks>
        /// A few common, generic codes are sorted explicitly to the front of the list,
        /// with all remaining codes alphabetical after them.
        /// </remarks>
        /// <param name="vals">enumerable return codes</param>
        /// <returns>enumerable of return codes in the desired order</returns>
        public override IEnumerable<string> SortReturnCodes(IEnumerable<string> vals)
            // => vals.OrderBy(GetKey);
            => vals.OrderByDescending(GetKey);


        private Tuple<int, string> GetKey(string item)
        {
            if (importance.TryGetValue(item, out var ret))
            {
                return ret;
            }
            return Tuple.Create(0, item);
        }
    }
}
