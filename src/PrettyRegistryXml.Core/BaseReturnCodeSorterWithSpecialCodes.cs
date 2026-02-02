// Copyright 2021-2026 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly ImportanceStringComparer comparer = new();

        /// <value>Your "special" codes, in the order you want them to appear.</value>
        /// <remarks>This is public for ease of unit testing</remarks>
        public abstract IEnumerable<string> PresortedSpecialCodes { get; }

        /// <summary>
        /// Constructor - processes your PresorterSpecialCodes
        /// </summary>
        public BaseReturnCodeSorterWithSpecialCodes() =>
            // Construct dictionary from IEnumerable<KeyValuePair>
            importance = new(PresortedSpecialCodes
                                // reverse so that later codes get a smaller index
                                .Reverse()
                                // turn codes into a key-value pair: mapping a code to a (negated-decreased-reverse-index, code) tuple
                                .Select((code, index) => KeyValuePair.Create(code, (-index - 100, code).ToTuple())));


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
            => vals.OrderBy(GetKey, comparer);

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
