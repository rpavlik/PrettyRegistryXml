// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrettyRegistryXml.Core;

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

        // public bool AppliesToElement(IEnumerable<string> elementAttrNames)
        // {
        //     return (from attrName in elementAttrNames
        //             where AttributeNameSet.Contains(attrName)
        //             select true).Any();
        // }

        private class WidthComputer : IAttributeSequenceItemWidthComputer
        {
            private AttributeGroup attrGroup;

            public WidthComputer(AttributeGroup attrGroup) => this.attrGroup = attrGroup;

            private List<NameLengthPair> observedLengths = new();
            public IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes)
            {
                // TODO might need to adjust - this only looks for adjacent items, across unlimited sets
                var mine = attributes.TakeWhile(NameLengthPair => attrGroup.AttributeNameSet.Contains(NameLengthPair.Name));
                observedLengths.AddRange(mine);
                return attributes.TakeLast(attributes.Count() - mine.Count());
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
