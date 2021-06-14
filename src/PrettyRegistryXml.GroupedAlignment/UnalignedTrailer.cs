// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable
using System.Linq;
using System.Collections.Generic;

namespace PrettyRegistryXml.GroupedAlignment
{

    /// <summary>
    /// Unaligned handling of the trailing attributes.
    /// </summary>
    public class UnalignedTrailer : AttributeSequenceTrailerBase
    {

        /// <inheritdoc />
        public override IAttributeSequenceItemWidthComputer CreateWidthComputer() => new WidthComputer(this);

        private class WidthComputer : IAttributeSequenceItemWidthComputer
        {
            private UnalignedTrailer attrGroup;

            public WidthComputer(UnalignedTrailer attrGroup) => this.attrGroup = attrGroup;

            private List<string[]> attributeNameOrders = new();
            private List<NameLengthPair> observedLengths = new();
            public IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes)
            {
                // takes all remaining
                observedLengths.AddRange(attributes);
                attributeNameOrders.Add((from pair in attributes
                                         select pair.Name).ToArray());
                return Enumerable.Empty<NameLengthPair>();
            }

            public IAttributeSequenceItemAligner Finish()
            {
                string[] biggestAttrList = (from attrList in attributeNameOrders
                                            orderby attrList.Count() descending
                                            select attrList).First();
                var alignedNamesSet = biggestAttrList.ToHashSet();
                Dictionary<string, int> lengthDictionary = new(from pair in observedLengths
                                                               group pair.Length by pair.Name into g
                                                               select KeyValuePair.Create(g.Key, g.Max()));
                var leftoverNames =
                    from a in lengthDictionary
                    where !alignedNamesSet.Contains(a.Key)
                    select a.Key;
                var alignments = (from name in biggestAttrList
                                  let length = lengthDictionary.GetValueOrDefault(name, 0)
                                  select new Core.AttributeAlignment(name, length));
                var leftoverNonAlignments = (from name in leftoverNames
                                             select Core.AttributeAlignment.MakeUnaligned(name));
                return new BaseAligner(alignments.Concat(leftoverNonAlignments).ToArray());
            }
        }

    }
}
