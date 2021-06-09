// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using PrettyRegistryXml.Core;

namespace PrettyRegistryXml.OpenXR
{
    /// <summary>
    /// A utility class for OpenXR's policy for sorting return codes.
    /// </summary>
    public sealed class ReturnCodeSorter : BaseReturnCodeSorterWithSpecialCodes
    {
        private static string[] _specialPresorted = new string[]{
                // These codes will be sorted first, in this order.
                "XR_SUCCESS",
                "XR_SESSION_LOSS_PENDING",
                "XR_ERROR_FUNCTION_UNSUPPORTED",
                "XR_ERROR_VALIDATION_FAILURE",
                "XR_ERROR_RUNTIME_FAILURE",
                "XR_ERROR_HANDLE_INVALID",
                "XR_ERROR_INSTANCE_LOST",
                "XR_ERROR_SESSION_LOST",
                "XR_ERROR_OUT_OF_MEMORY",
                "XR_ERROR_LIMIT_REACHED",
                "XR_ERROR_SIZE_INSUFFICIENT",
            };

        public override IEnumerable<string> PresortedSpecialCodes => _specialPresorted;
    }
}
