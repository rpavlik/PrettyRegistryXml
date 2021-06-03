// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace PrettyRegistryXml.OpenXR
{
    public sealed class ReturnCodeSorter
    {
        private readonly Dictionary<string, Tuple<string, int>> importance = new string[]{
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
            }.Reverse()
             .Select((item, index) => (item, index + 100).ToTuple())
             .ToDictionary(tup => tup.Item1, tup => tup);


        public Tuple<string, int> GetKey(string item)
        {
            if (!importance.TryGetValue(item, out var ret))
            {
                ret = (item, 0).ToTuple();
            }
            return ret;
        }

        public string SortReturnCodeString(string vals)
            => string.Join(',', vals.Split(',', StringSplitOptions.RemoveEmptyEntries).OrderByDescending(GetKey));
    }
}
