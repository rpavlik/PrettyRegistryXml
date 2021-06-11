// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using Xunit;

namespace PrettyRegistryXml.GroupedAlignment.Tests
{
    public class GroupedAlignmentTest
    {
        [Fact]
        public void Test1()
        {
            var choice = new GroupChoice(new AttributeGroup("value"),
                                         new AttributeGroup("offset", "dir", "extends"));
        }
    }
}
