// Copyright 2021-2026 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using PrettyRegistryXml.Core;
using System.Collections.Generic;

namespace PrettyRegistryXml.OpenXR
{
    /// <summary>
    /// A utility class for OpenXR's policy for sorting return codes.
    /// </summary>
    public sealed class ReturnCodeSorter : BaseReturnCodeSorterWithSpecialCodes
    {
        private static readonly string[] _specialPresorted = [
                // These codes will be sorted first, in this order.
                "XR_SUCCESS",
            ];

        /// <inheritdoc />
        public override IEnumerable<string> PresortedSpecialCodes => _specialPresorted;
    }
}
