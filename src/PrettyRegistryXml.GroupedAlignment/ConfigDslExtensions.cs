// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace PrettyRegistryXml.GroupedAlignment
{

    public class GroupChoiceBuilder
    {
        public List<AttributeGroup> Groups = new();
        public GroupChoiceBuilder Or(params string[] attributeNames)
        {
            Groups.Add(new AttributeGroup(attributeNames));
            return this;
        }
        public static implicit operator GroupChoice(GroupChoiceBuilder builder) => new GroupChoice(builder.Groups);
    }
    public class Column
    {
        public static AttributeGroup Containing(params string[] attributeNames) => new AttributeGroup(attributeNames);
    }
    public static class ConfigDslExtensions
    {
        public static GroupChoiceBuilder Or(this AttributeGroup attributeGroup, params string[] attributeNames)
        {
            var builder = new GroupChoiceBuilder();
            builder.Groups.Add(attributeGroup);
            builder.Groups.Add(new AttributeGroup(attributeNames));
            return builder;
        }
    }
}
