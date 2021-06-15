// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace PrettyRegistryXml.GroupedAlignment
{

    /// <summary>
    /// An object for creating <see cref="GroupChoice"/> by a DSL.
    /// Created by a call to <see cref="ConfigDslExtensions.Or(AttributeGroup, string[])"/>.
    /// </summary>
    public class GroupChoiceBuilder
    {
        readonly List<AttributeGroup> _groups = new();

        /// <summary>
        /// Add an alternative list of attribute names for alignment.
        /// </summary>
        /// <param name="attributeNames">Attribute name strings</param>
        /// <returns>itself</returns>
        public GroupChoiceBuilder Or(params string[] attributeNames)
        {
            return Or(new AttributeGroup(attributeNames));
        }

        /// <summary>
        /// Add an alternative attribute group for alignment.
        /// </summary>
        /// <param name="attributeGroup">An attribute group</param>
        /// <returns>itself</returns>
        public GroupChoiceBuilder Or(AttributeGroup attributeGroup)
        {
            _groups.Add(attributeGroup);
            return this;
        }

        /// <summary>
        /// Conversion to <see cref="GroupChoice"/>
        /// </summary>
        /// <param name="builder">The builder to convert into a <see cref="GroupChoice"/></param>
        public static implicit operator GroupChoice(GroupChoiceBuilder builder) => new(builder._groups);
    }

    /// <summary>
    /// Represents an aligned column containing one or more <see cref="AttributeGroup"/> objects.
    /// </summary>
    public class Column
    {
        /// <summary>
        /// Specify the attributes to include in this column
        /// </summary>
        /// <param name="attributeNames">Attribute name strings</param>
        /// <returns>An <see cref="AttributeGroup"/></returns>
        public static AttributeGroup Containing(params string[] attributeNames) => new(attributeNames);
    }

    /// <summary>
    /// Extension class for accessing a DSL-style way of specifying alignment.
    /// </summary>
    public static class ConfigDslExtensions
    {
        /// <summary>
        /// Starts accumulating a <see cref="GroupChoiceBuilder"/> by adding an alternate <see cref="AttributeGroup"/>
        /// </summary>
        /// <param name="attributeGroup">A first attribute group, perhaps from <see cref="Column.Containing(string[])"/></param>
        /// <param name="attributeNames">Alternate set of attribute name strings</param>
        /// <returns>A <see cref="GroupChoiceBuilder"/> on which <see cref="GroupChoiceBuilder.Or(string[])"/> may be called again to add more alternatives</returns>
        public static GroupChoiceBuilder Or(this AttributeGroup attributeGroup, params string[] attributeNames)
        {
            return new GroupChoiceBuilder().Or(attributeGroup)
                                           .Or(new AttributeGroup(attributeNames));
        }
    }
}
