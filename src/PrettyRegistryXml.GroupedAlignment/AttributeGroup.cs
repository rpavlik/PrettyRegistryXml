// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrettyRegistryXml.Core;
using static MoreLinq.Extensions.PartitionExtension;

namespace PrettyRegistryXml.GroupedAlignment
{
    /// <summary>
    /// A list of attribute names, usually combined in a <see cref="GroupChoice"/>
    /// </summary>
    public class AttributeGroup : AttributeSequenceItemBase
    {
        /// <value>The attribute names in the desired order</value>
        public string[] AttributeNames { get; init; }

        /// <value>A <see cref="HashSet{T}"/> of the elements of <see cref="AttributeNames"/> </value>
        public HashSet<string> AttributeNameSet { get; private init; }

        /// <summary>
        /// Create a group of attributes that will all be aligned (or replaced with placeholder spaces)
        /// </summary>
        /// <param name="attributeNames">Attribute names in the desired order</param>
        public AttributeGroup(params string[] attributeNames)
        {
            AttributeNames = attributeNames.ToArray();
            AttributeNameSet = AttributeNames.ToHashSet();
        }


        /// <inheritdoc />
        public override int CountHandledAttributes(IEnumerable<string> elementAttrNames) => (from attrName in elementAttrNames
                                                                                             where AttributeNameSet.Contains(attrName)
                                                                                             select true).Count();

        private class WidthComputer : IAttributeSequenceItemWidthComputer
        {
            private AttributeGroup attrGroup;

            public WidthComputer(AttributeGroup attrGroup) => this.attrGroup = attrGroup;

            private List<NameLengthPair> observedLengths = new();
            public IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes)
            {
                var (selected, notSelected) = attributes.Partition(attr => attrGroup.AttributeNameSet.Contains(attr.Name));
                observedLengths.AddRange(selected);
                return notSelected;
            }

            public IAttributeSequenceItemAligner Finish()
            {

                Dictionary<string, int> lengthDictionary = new(from pair in observedLengths
                                                               group pair.Length by pair.Name into g
                                                               select KeyValuePair.Create(g.Key, g.Max()));
                var alignments = (from name in attrGroup.AttributeNames
                                  let length = lengthDictionary.GetValueOrDefault(name, 0)
                                  select new Core.AttributeAlignment(name, length)).ToArray();
                return new BaseAligner(alignments);
            }
        }

        /// <inheritdoc />
        public override IAttributeSequenceItemWidthComputer CreateWidthComputer() => new WidthComputer(this);
    }

}
