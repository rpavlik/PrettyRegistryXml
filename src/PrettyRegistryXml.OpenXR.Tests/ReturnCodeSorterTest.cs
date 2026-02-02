// Copyright 2021-2026 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PrettyRegistryXml.OpenXR.Tests
{
    public class ReturnCodeSorterTest
    {
        private static readonly ReturnCodeSorter Sorter = new();
        private static readonly string SpecialCodesStringInOrder = string.Join(',', Sorter.PresortedSpecialCodes);

#pragma warning disable CA1861 // Avoid constant arrays as arguments
        public static IEnumerable<TheoryDataRow<string[], string[]>> SmallData => [
            // not special, underscore sorting
            new TheoryDataRow<string[], string[]>(
                // expected
                ["XR_ERROR_ACTIONSET_NOT_ATTACHED", "XR_ERROR_ACTION_TYPE_MISMATCH"],
                // unsorted
                ["XR_ERROR_ACTION_TYPE_MISMATCH", "XR_ERROR_ACTIONSET_NOT_ATTACHED"]
            ),
            // special codes
            new TheoryDataRow<string[], string[]>(
                // expected
                [Sorter.PresortedSpecialCodes.First(), "XR_SESSION_LOSS_PENDING",],
                // unsorted
                ["XR_SESSION_LOSS_PENDING", Sorter.PresortedSpecialCodes.First(),]
            ),
        ];
#pragma warning restore CA1861 // Avoid constant arrays as arguments
        [Theory]
        [MemberData(nameof(SmallData))]
        public void SortSmallSpecialData(string[] expected, string[] unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodes(unsorted));
        }

        public static IEnumerable<TheoryDataRow<string, string>> RealStringData => [
            new TheoryDataRow<string, string>(
                // from xrGetActionStateVector2f
                // expected
                "XR_ERROR_ACTIONSET_NOT_ATTACHED,XR_ERROR_ACTION_TYPE_MISMATCH,XR_ERROR_HANDLE_INVALID,XR_ERROR_INSTANCE_LOST,XR_ERROR_PATH_INVALID,XR_ERROR_PATH_UNSUPPORTED,XR_ERROR_RUNTIME_FAILURE,XR_ERROR_SESSION_LOST,XR_ERROR_VALIDATION_FAILURE",
                // unsorted
                "XR_ERROR_INSTANCE_LOST,XR_ERROR_SESSION_LOST,XR_ERROR_RUNTIME_FAILURE,XR_ERROR_HANDLE_INVALID,XR_ERROR_ACTIONSET_NOT_ATTACHED,XR_ERROR_ACTION_TYPE_MISMATCH,XR_ERROR_VALIDATION_FAILURE,XR_ERROR_PATH_INVALID,XR_ERROR_PATH_UNSUPPORTED"
            ),
        ];

        [Theory]
        [MemberData(nameof(RealStringData))]
        public void SortRealStringData(string expected, string unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodeString(unsorted));

        }

#pragma warning disable CA1861 // Avoid constant arrays as arguments
        public static IEnumerable<TheoryDataRow<string[], string[]>> RealData => [
            new TheoryDataRow<string[], string[]>(
                // from xrGetActionStateVector2f
                // expected
                [
                    "XR_ERROR_ACTIONSET_NOT_ATTACHED",
                    "XR_ERROR_ACTION_TYPE_MISMATCH",
                    "XR_ERROR_HANDLE_INVALID",
                    "XR_ERROR_INSTANCE_LOST",
                    "XR_ERROR_PATH_INVALID",
                    "XR_ERROR_PATH_UNSUPPORTED",
                    "XR_ERROR_RUNTIME_FAILURE",
                    "XR_ERROR_SESSION_LOST",
                    "XR_ERROR_VALIDATION_FAILURE",
                ],
                // unsorted
                [
                    "XR_ERROR_VALIDATION_FAILURE",
                    "XR_ERROR_RUNTIME_FAILURE",
                    "XR_ERROR_HANDLE_INVALID",
                    "XR_ERROR_INSTANCE_LOST",
                    "XR_ERROR_SESSION_LOST",
                    "XR_ERROR_PATH_UNSUPPORTED",
                    "XR_ERROR_PATH_INVALID",
                    "XR_ERROR_ACTION_TYPE_MISMATCH",
                    "XR_ERROR_ACTIONSET_NOT_ATTACHED"
                ]
            ),
        ];
#pragma warning restore CA1861 // Avoid constant arrays as arguments

        [Theory]
        [MemberData(nameof(RealData))]
        public void SortRealData(string[] expected, string[] unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodes(unsorted));
        }

        public static TheoryData<IEnumerable<string>> AllSpecialCodes => new() {
            Sorter.PresortedSpecialCodes,
            Sorter.PresortedSpecialCodes.Reverse(),
        };

        [Theory]
        [MemberData(nameof(AllSpecialCodes))]
        public void SortSpecialCodes(IEnumerable<string> value)
        {
            // first do no harm
            Assert.Equal(Sorter.PresortedSpecialCodes, Sorter.SortReturnCodes(value));
        }

        public static TheoryData<string> AllSpecialCodesStrings => [
            SpecialCodesStringInOrder,
            string.Join(',', Sorter.PresortedSpecialCodes.Reverse()),
            // with empty items between
            string.Join(",,", Sorter.PresortedSpecialCodes),
            string.Join(",,", Sorter.PresortedSpecialCodes.Reverse()),
        ];

        [Theory]
        [MemberData(nameof(AllSpecialCodesStrings))]
        public void SortAllSpecialCodeString(string value)
        {

            // first do no harm
            Assert.Equal(SpecialCodesStringInOrder, Sorter.SortReturnCodeString(value));
        }
    }
}
