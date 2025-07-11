// Copyright 2021-2025 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static MoreLinq.Extensions.PartitionExtension;

namespace PrettyRegistryXml.GroupedAlignment
{
    /// <summary>
    /// A list of attribute names, usually combined in a <see cref="GroupChoice"/>.
    /// They will all be aligned together.
    /// </summary>
    public class AttributeGroup : AttributeSequenceItemBase
    {
        /// <value>The attribute names in the desired order</value>
        public string[] AttributeNames { get; init; }

        /// <value>A <see cref="HashSet{T}"/> of the elements of <see cref="AttributeNames"/> </value>
        public HashSet<string> AttributeNameSet { get; private init; }

        /// <summary>
        /// Extra space to add to this attribute group's width.
        /// </summary>
        public int ExtraSpace { get; private init; }

        /// <summary>
        /// Create a group of attributes that will all be aligned (or replaced with placeholder spaces)
        /// </summary>
        /// <param name="attributeNames">Attribute names in the desired order</param>
        public AttributeGroup(params string[] attributeNames)
        {
            AttributeNames = attributeNames.ToArray();
            AttributeNameSet = AttributeNames.ToHashSet();
            ExtraSpace = 0;
        }

        /// <summary>
        /// Create a group of attributes that will all be aligned (or replaced with placeholder spaces)
        /// </summary>
        /// <param name="extraSpace">Extra space</param>
        /// <param name="attributeNames">Attribute names in the desired order</param>
        public AttributeGroup(int extraSpace, params string[] attributeNames)
        {
            AttributeNames = attributeNames.ToArray();
            AttributeNameSet = AttributeNames.ToHashSet();
            ExtraSpace = extraSpace;
        }

        /// <inheritdoc />
        public override int CountHandledAttributes(IEnumerable<string> elementAttrNames) => (from attrName in elementAttrNames
                                                                                             where AttributeNameSet.Contains(attrName)
                                                                                             select true).Count();

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>Representation mostly for debugging</returns>
        public override string? ToString() => string.Format(CultureInfo.InvariantCulture,
                                                            "AttributeGroup( {0} )",
                                                            string.Join(", ", from name in AttributeNames
                                                                              select $"\"{name}\""));

        private sealed class WidthComputer : IAttributeSequenceItemWidthComputer
        {
            private readonly AttributeGroup attrGroup;

            public WidthComputer(AttributeGroup attrGroup) => this.attrGroup = attrGroup;

            private readonly List<NameLengthPair> observedLengths = new();
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
                                                               select KeyValuePair.Create(g.Key, g.Max() + attrGroup.ExtraSpace));
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
