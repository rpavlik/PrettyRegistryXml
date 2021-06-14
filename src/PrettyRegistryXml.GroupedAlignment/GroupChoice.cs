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

        /// <summary>
        /// Create a group choice.
        /// </summary>
        /// <param name="groups">Two or more <see cref="AttributeGroup"/> objects,
        /// to alternate between.</param>
        public GroupChoice(params AttributeGroup[] groups)
        {
            Groups = groups;
        }

        private (AttributeGroup, int) FindBestMatchingGroup(IEnumerable<string> elementAttrNames)
        {
            // find the option that handles the most.
            var q = from g in Groups
                    let numHandled = g.CountHandledAttributes(elementAttrNames)
                    orderby numHandled descending
                    select (g, numHandled);
            return q.First();
        }

        /// <inheritdoc />
        public override int CountHandledAttributes(IEnumerable<string> elementAttrNames) => FindBestMatchingGroup(elementAttrNames).Item2;

        /// <inheritdoc />
        public override IAttributeSequenceItemWidthComputer CreateWidthComputer() => new WidthComputer(this, Groups);
    }
}
