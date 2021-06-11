// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;


namespace PrettyRegistryXml.GroupedAlignment
{
    /// <summary>
    /// A choice between some disjoint collections of attribute names
    /// represented by <see cref="AttributeGroup"/>
    /// </summary>
    public partial class GroupChoice : AttributeSequenceItemBase
    {
        private AttributeGroup[] Groups { get; init; }

        private readonly Dictionary<string, AttributeGroup> nameToGroup;

        /// <summary>
        /// Create a group choice.
        /// </summary>
        /// <param name="groups">Two or more <see cref="AttributeGroup"/> objects,
        /// with disjoint attribute names, to alternate between.</param>
        public GroupChoice(params AttributeGroup[] groups)
        {
            Groups = groups;
            nameToGroup = new(from g in Groups
                              from attrName in g.AttributeNames
                              select KeyValuePair.Create(attrName, g));
        }

        /// <inheritdoc />
        public override IAttributeSequenceItemWidthComputer CreateWidthComputer() => new WidthComputer(this, Groups);
    }
}
