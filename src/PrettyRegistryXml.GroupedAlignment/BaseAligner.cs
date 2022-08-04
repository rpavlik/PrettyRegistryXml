// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable
using PrettyRegistryXml.Core;
using System.Collections.Generic;
using System.Linq;
using static MoreLinq.Extensions.PartitionExtension;

namespace PrettyRegistryXml.GroupedAlignment
{
    internal class BaseAligner : IAttributeSequenceItemAligner
    {
        public readonly AttributeAlignment[] alignments;
        private readonly HashSet<string> knownNames;

        public BaseAligner(AttributeAlignment[] alignments)
        {
            this.alignments = alignments;
            knownNames = (from align in alignments
                          select align.Name).ToHashSet();
        }


        /// <inheritdoc />
        public int FullWidth => (from align in alignments
                                 where align.ShouldAlign
                                 select align.FullWidth).Sum();

        /// <summary>
        /// Split attribute names into selected (they're in our list of attribute alignments) and not,
        /// keeping their original order.
        /// </summary>
        /// <param name="attributeNames">Input enumerable of attribute names</param>
        /// <param name="selected">Attribute names found in our alignments, in their order from <paramref name="attributeNames"/></param>
        /// <param name="notSelected">Attribute names not found in our alignments, in their order from <paramref name="attributeNames"/></param>
        public void SplitAttributeNamesKeepingOrder(IEnumerable<string> attributeNames,
                                                    out IEnumerable<string> selected,
                                                    out IEnumerable<string> notSelected)
        {
            (selected, notSelected) = attributeNames.Partition(name => knownNames.Contains(name));
        }

        /// <inheritdoc />
        public bool TakeAndHandleAttributes(IEnumerable<string> attributeNames,
                                            out IEnumerable<AttributeAlignment> alignments,
                                            out IEnumerable<string> remainingNames)
        {
            SplitAttributeNamesKeepingOrder(attributeNames, out _, out var notSelected);
            alignments = this.alignments;
            remainingNames = notSelected;
            return true;
        }
    }
}
