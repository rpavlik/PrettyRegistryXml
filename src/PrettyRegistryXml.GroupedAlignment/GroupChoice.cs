// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PrettyRegistryXml.GroupedAlignment
{
    /// <summary>
    /// A choice between some disjoint collections of attribute names
    /// represented by <see cref="AttributeGroup"/>
    /// </summary>
    /// <remarks>
    /// Create a group choice.
    /// </remarks>
    /// <param name="groups">Two or more <see cref="AttributeGroup"/> objects,
    /// to alternate between.</param>
    public partial class GroupChoice(params AttributeGroup[] groups) : AttributeSequenceItemBase
    {
        private AttributeGroup[] Groups { get; init; } = groups;

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

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>Representation mostly for debugging</returns>
        public override string? ToString() => string.Format(CultureInfo.InvariantCulture,
                                                            "GroupChoice( {0} )",
                                                            string.Join(", ",
                                                                        from g in Groups
                                                                        select g.ToString()));
    }
}
