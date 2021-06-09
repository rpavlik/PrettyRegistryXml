// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Linq;
using System.Collections.Generic;
using Xunit;
using System;

namespace PrettyRegistryXml.OpenXR.Tests
{
    public class ReturnCodeSorterTest
    {
        public static ReturnCodeSorter Sorter = new ReturnCodeSorter();
        public static string SpecialCodesStringInOrder = string.Join(',', Sorter.PresortedSpecialCodes);

        public static IEnumerable<object[]> SmallData => new List<object[]>{
            // not special, underscore sorting
            new object[]{
                // expected - what the python does (when sorting reverse!)
                new string[]{"XR_ERROR_ACTION_TYPE_MISMATCH", "XR_ERROR_ACTIONSET_NOT_ATTACHED"},
                // unsorted
                new string[]{"XR_ERROR_ACTIONSET_NOT_ATTACHED", "XR_ERROR_ACTION_TYPE_MISMATCH"},
            },
            // special codes
            new object[]{
                // expected
                Sorter.PresortedSpecialCodes.Take(2).ToArray(),
                // unsorted
                Sorter.PresortedSpecialCodes.Take(2).Reverse().ToArray()
            },
        };
        [Theory]
        [MemberData(nameof(SmallData))]
        public void SortSmallSpecialData(string[] expected, string[] unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodes(unsorted));
        }

        public static IEnumerable<object[]> RealStringData => new List<object[]>{
            new object[]{
                // from xrGetActionStateVector2f
                // expected
                "XR_ERROR_VALIDATION_FAILURE,XR_ERROR_RUNTIME_FAILURE,XR_ERROR_HANDLE_INVALID,XR_ERROR_INSTANCE_LOST,XR_ERROR_SESSION_LOST,XR_ERROR_PATH_UNSUPPORTED,XR_ERROR_PATH_INVALID,XR_ERROR_ACTION_TYPE_MISMATCH,XR_ERROR_ACTIONSET_NOT_ATTACHED",
                // unsorted
                "XR_ERROR_INSTANCE_LOST,XR_ERROR_SESSION_LOST,XR_ERROR_RUNTIME_FAILURE,XR_ERROR_HANDLE_INVALID,XR_ERROR_ACTIONSET_NOT_ATTACHED,XR_ERROR_ACTION_TYPE_MISMATCH,XR_ERROR_VALIDATION_FAILURE,XR_ERROR_PATH_INVALID,XR_ERROR_PATH_UNSUPPORTED"
            },
        };

        [Theory]
        [MemberData(nameof(RealStringData))]
        public void SortRealStringData(string expected, string unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodeString(unsorted));

        }
        public static IEnumerable<object[]> RealData => new List<object[]>{
            new object[]{
                // from xrGetActionStateVector2f
                // expected
                new string[]{"XR_ERROR_VALIDATION_FAILURE", "XR_ERROR_RUNTIME_FAILURE", "XR_ERROR_HANDLE_INVALID", "XR_ERROR_INSTANCE_LOST", "XR_ERROR_SESSION_LOST", "XR_ERROR_PATH_UNSUPPORTED", "XR_ERROR_PATH_INVALID", "XR_ERROR_ACTION_TYPE_MISMATCH", "XR_ERROR_ACTIONSET_NOT_ATTACHED"},
                // unsorted
                new string[]{"XR_ERROR_INSTANCE_LOST", "XR_ERROR_SESSION_LOST", "XR_ERROR_RUNTIME_FAILURE", "XR_ERROR_HANDLE_INVALID", "XR_ERROR_ACTIONSET_NOT_ATTACHED", "XR_ERROR_ACTION_TYPE_MISMATCH", "XR_ERROR_VALIDATION_FAILURE", "XR_ERROR_PATH_INVALID", "XR_ERROR_PATH_UNSUPPORTED"}
            },
        };

        [Theory]
        [MemberData(nameof(RealData))]
        public void SortRealData(string[] expected, string[] unsorted)
        {
            Assert.Equal(expected, Sorter.SortReturnCodes(unsorted));

        }

        public static IEnumerable<object[]> AllSpecialCodes => new List<object[]>{
            new object[]{Sorter.PresortedSpecialCodes},
            new object[]{Sorter.PresortedSpecialCodes.Reverse()},
        };

        [Theory]
        [MemberData(nameof(AllSpecialCodes))]
        public void SortSpecialCodes(IEnumerable<string> value)
        {
            // first do no harm
            Assert.Equal(Sorter.PresortedSpecialCodes, Sorter.SortReturnCodes(value));
        }

        public static IEnumerable<object[]> AllSpecialCodesStrings => new List<object[]>{
            new object[]{SpecialCodesStringInOrder},
            new object[]{string.Join(',', Sorter.PresortedSpecialCodes.Reverse())},
            // with empty items between
            new object[]{string.Join(",,", Sorter.PresortedSpecialCodes)},
            new object[]{string.Join(",,", Sorter.PresortedSpecialCodes.Reverse())},
        };

        [Theory]
        [MemberData(nameof(AllSpecialCodesStrings))]
        public void SortAllSpecialCodeString(string value)
        {

            // first do no harm
            Assert.Equal(SpecialCodesStringInOrder, Sorter.SortReturnCodeString(value));
        }
    }
}
