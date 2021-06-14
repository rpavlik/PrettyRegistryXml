// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using PrettyRegistryXml.Core;

namespace PrettyRegistryXml.GroupedAlignment
{

    /// <summary>
    /// A more complex alignment: there are groups of attributes that may alternate.
    /// </summary>
    public class GroupedAttributeAlignment : IAlignmentFinder
    {
        private IAttributeSequenceItem MakeDefaultTrailer() => new AlignedTrailer();
        private readonly IAttributeSequenceItem[] attributeSequenceItems;

        /// <summary>
        /// Create an alignment of this type.
        /// </summary>
        /// <remarks>
        /// If you do not pass a "trailer" as your last element
        /// (an item for which <see cref="IAttributeSequenceItem.IsTrailer"/> is true,
        /// indicating it will consume all remaining attributes),
        /// one will be automatically added for you.
        /// It is an error to have one anywhere besides the last element.
        /// </remarks>
        /// <param name="attributeSequenceItems">At least one <see cref="IAttributeSequenceItem"/>-implementing object</param>
        public GroupedAttributeAlignment(params IAttributeSequenceItem[] attributeSequenceItems)
        {
            if (!attributeSequenceItems.Any())
            {
                throw new ArgumentOutOfRangeException("Need at least one attribute sequence item");
            }
            if (attributeSequenceItems.Last().IsTrailer)
            {
                // a trailer was explicitly provided
                this.attributeSequenceItems = attributeSequenceItems;
            }
            else
            {
                // add a default trailer
                this.attributeSequenceItems = attributeSequenceItems.Append(MakeDefaultTrailer()).ToArray();
            }
            // If we have more than one trailer (and thus have any trailers anywhere before the last item),
            // that was a configuration error.
            if (this.attributeSequenceItems.Where(item => item.IsTrailer).Skip(1).Any())
            {
                throw new ArgumentException("Cannot have a trailer anywhere except the last item.");
            }
        }



        /// <summary>
        /// Find the alignment state for a collection of elements.
        /// </summary>
        public IAlignmentState FindAlignment(IEnumerable<XElement> elements)
        {
            var widthComputers = (from sequenceItem in attributeSequenceItems
                                  select sequenceItem.CreateWidthComputer()).ToArray();

            foreach (var element in elements)
            {
                ProcessElementAttributes(widthComputers, element);
            }
            var aligners = from state in widthComputers
                           select state.Finish();
            int elementNameAlignment = ElementAlignment.FindMaxNameAlignment(elements);
            return new State(elementNameAlignment, aligners);
        }

        private static void ProcessElementAttributes(IAttributeSequenceItemWidthComputer[] widthComputers, XElement element)
        {
            var attributeNamesAndLengths = (from attr in element.Attributes()
                                            select new NameLengthPair(attr)).ToArray();
            IEnumerable<NameLengthPair> remaining = attributeNamesAndLengths;
            foreach (IAttributeSequenceItemWidthComputer widthComputer in widthComputers)
            {
                remaining = widthComputer.TakeAndHandleAttributes(remaining);
            }
            if (remaining.Any())
            {
                throw new ArgumentOutOfRangeException(
                    $"We got an attribute name left over after the last configured item - maybe need a Leftover item. '{remaining.First()}'");
            }
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns>Representation mostly for debugging</returns>
        public override string? ToString() => string.Format("GroupedAttributeAlignment( {0} )",
                                                            string.Join(", ",
                                                                        from sequenceItem in attributeSequenceItems
                                                                        select sequenceItem.ToString()));

        private class State : IAlignmentState
        {
            private readonly int ElementNameAlignment;
            private readonly IAttributeSequenceItemAligner[] aligners;

            public State(int elementNameAlignment, IEnumerable<IAttributeSequenceItemAligner> aligners)
            {
                ElementNameAlignment = elementNameAlignment;
                this.aligners = aligners.ToArray();
            }

            private IEnumerable<IEnumerable<AttributeAlignment>> HandleAttribute(IEnumerable<string> attributeNames)
            {
                IEnumerable<string> remaining = attributeNames;

                foreach (var aligner in aligners)
                {
                    if (aligner.TakeAndHandleAttributes(remaining, out var alignments, out var newRemaining))
                    {
                        yield return alignments;
                        remaining = newRemaining;
                    }
                }
                if (remaining.Any())
                {
                    throw new ArgumentOutOfRangeException("We got an attribute name in padding we didn't see in scanning? " + remaining.First());
                }
            }

            public IEnumerable<AttributeAlignment> DetermineAlignment(IEnumerable<string> attributeNames)
            {
                // flatten
                var flattenedAlignments = from inner in HandleAttribute(attributeNames)
                                          from value in inner
                                          select value;
#if DEBUG
                var alignments = flattenedAlignments.ToArray();
                var q = from a in alignments
                        group a by a.Name into g
                        let count = g.Count()
                        orderby count descending
                        select (g.Key, count);

                var top = q.First();
                Debug.Assert(top.count < 2);
                return alignments;
#else
                return flattenedAlignments;
#endif
            }

            public int ComputeElementPaddingWidth(XElement element)
                => ElementAlignment.ComputeElementPaddingWidth(ElementNameAlignment, element);

        }
    }
}
