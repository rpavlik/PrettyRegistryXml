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
    /// Utility class for sorting return codes. The default is to sort all alphabetically, removing empty entries.
    /// </summary>
    /// <remarks>
    /// This also serves as a base class for sorters with more sophisticated policies.
    /// </remarks>
    public abstract class BaseReturnCodeSorter
    {
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
            => string.Join(',', SortReturnCodes(vals.Split(',', StringSplitOptions.RemoveEmptyEntries)));

        /// <summary>
        /// Sorts an enumerable of return codes.
        /// </summary>
        /// <param name="vals">enumerable return codes</param>
        /// <returns>enumerable of return codes in alphabetical order (by default)</returns>
        public virtual IEnumerable<string> SortReturnCodes(IEnumerable<string> vals) => vals.OrderBy(x => x);
    }
}
