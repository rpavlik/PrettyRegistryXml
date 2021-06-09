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

        [Fact]
        public void StringComparison()
        {
            // aka "XR_ERROR_ACTION_TYPE_MISMATCH" > "XR_ERROR_ACTIONSET_NOT_ATTACHED"
            Assert.True(("XR_ERROR_ACTION_TYPE_MISMATCH" as IComparable).CompareTo("XR_ERROR_ACTIONSET_NOT_ATTACHED") > 0);
        }

        [Fact]
        public void TupleComparison()
        {
            {
                // Tuple<int, string> a = (101, "XR_ERROR_LIMIT_REACHED").ToTuple();
                // Tuple<int, string> b = (108, "XR_ERROR_FUNCTION_UNSUPPORTED").ToTuple();
                Tuple<int, string> a = (109, "XR_SESSION_LOSS_PENDING").ToTuple();
                Tuple<int, string> b = (110, "XR_SUCCESS").ToTuple();
                Assert.True((a as IComparable).CompareTo(b) < 0);
            }
            {
                var a = (0, "XR_ERROR_ACTIONSET_NOT_ATTACHED").ToTuple();
                var b = (0, "XR_ERROR_ACTION_TYPE_MISMATCH").ToTuple();
                Assert.Equal(new Tuple<int, string>[] { a, b }, (new Tuple<int, string>[] { b, a }).OrderBy(val => val));
            }
            // Assert.Equal(  Sorter.SortReturnCodes(new string[]{a.Item2, b.Item2}))
        }

        public static IEnumerable<object[]> SmallData => new List<object[]>{
            // not special, underscore sorting
            new object[]{
                // expected - what the python does
                new string[]{"XR_ERROR_ACTIONSET_NOT_ATTACHED", "XR_ERROR_ACTION_TYPE_MISMATCH"},
                // unsorted
                new string[]{"XR_ERROR_ACTION_TYPE_MISMATCH", "XR_ERROR_ACTIONSET_NOT_ATTACHED"},
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
