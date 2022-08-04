// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable
using PrettyRegistryXml.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace PrettyRegistryXml.GroupedAlignment
{
    /// <summary>
    /// An attribute name and length
    /// </summary>
    public record NameLengthPair
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="attr">The attribute</param>
        public NameLengthPair(XAttribute attr)
        {
            Name = attr.Name.ToString();
            Length = AttributeAlignment.GetAttributeAlignLength(attr);
        }

        /// <value>The name of an attribute</value>
        public string Name { get; init; }


        /// <value>The length of an attribute</value>
        public int Length { get; init; }
    }

    /// <summary>
    /// The result of <see cref="IAttributeSequenceItemWidthComputer"/> when it is all done and ready to actually align.
    /// </summary>
    public interface IAttributeSequenceItemAligner
    {
        /// <value>The total aligned width of all attributes this item cares about</value>
        int FullWidth { get; }

        /// <summary>
        /// Process what we can, outputting appropriate alignments, and remove them from consideration.
        /// </summary>
        /// <param name="attributeNames">The unhandled attribute names left in an element</param>
        /// <param name="alignments">Output to be populated with alignments, if any apply.</param>
        /// <param name="remainingNames">A sequence containing items in <paramref name="attributeNames"/> we didn't handle, if any,
        /// preferably in their original order</param>
        /// <returns>true if we handled any and populated <paramref name="alignments"/></returns>
        bool TakeAndHandleAttributes(IEnumerable<string> attributeNames,
                                     [MaybeNullWhen(false)] out IEnumerable<AttributeAlignment> alignments,
                                     out IEnumerable<string> remainingNames);
    }

    /// <summary>
    /// The state of an <see cref="IAttributeSequenceItem"/> while it is
    /// determining the alignment widths.
    /// </summary>
    public interface IAttributeSequenceItemWidthComputer
    {
        /// <summary>
        /// Process what we can of one element's attributes, and remove them from consideration.
        /// </summary>
        /// <param name="attributes">The unhandled attributes left in an element</param>
        /// <returns>A sequence containing items in <paramref name="attributes"/> we didn't handle,
        /// preferably in their original order</returns>
        IEnumerable<NameLengthPair> TakeAndHandleAttributes(IEnumerable<NameLengthPair> attributes);

        /// <summary>
        /// Call when all done calling <see cref="TakeAndHandleAttributes(IEnumerable{NameLengthPair})"/>
        /// to get what you need to actually perform alignment.
        /// </summary>
        /// <returns>The aligner</returns>
        IAttributeSequenceItemAligner Finish();
    }

    /// <summary>
    /// Configuration for a single item in the attribute sequence,
    /// which may be a choice between groups of attributes,
    /// or just a group of attributes.
    /// </summary>
    public interface IAttributeSequenceItem
    {
        /// <value>true if this item consumes all remaining attributes and thus should be the last one specified</value>
        bool IsTrailer { get; }

        /// <summary>
        /// Create the object needed to process a collection of elements to determine alignment widths.
        /// </summary>
        IAttributeSequenceItemWidthComputer CreateWidthComputer();

        /// <summary>
        /// Computes the number of attributes in this list of names that we can handle.
        /// </summary>
        /// <param name="elementAttrNames">Attribute names</param>
        /// <returns>Number handled</returns>
        int CountHandledAttributes(IEnumerable<string> elementAttrNames);
    }

    /// <summary>
    /// Base class for non-trailer implementations of <see cref="IAttributeSequenceItem"/>
    /// </summary>
    public abstract class AttributeSequenceItemBase : IAttributeSequenceItem
    {
        /// <summary>
        /// This is not a trailer.
        /// </summary>
        public bool IsTrailer => false;

        /// <summary>
        /// Create the object needed to process a collection of elements to determine alignment widths.
        /// </summary>
        public abstract IAttributeSequenceItemWidthComputer CreateWidthComputer();

        /// <summary>
        /// Computes the number of attributes in this list of names that we can handle.
        /// </summary>
        /// <param name="elementAttrNames">Attribute names</param>
        /// <returns>Number handled</returns>
        public abstract int CountHandledAttributes(IEnumerable<string> elementAttrNames);
    }

    /// <summary>
    /// Base class for trailer implementations of <see cref="IAttributeSequenceItem"/>
    /// </summary>
    public abstract class AttributeSequenceTrailerBase : IAttributeSequenceItem
    {
        /// <summary>
        /// This is always a trailer.
        /// </summary>
        public bool IsTrailer => true;

        /// <summary>
        /// Create the object needed to process a collection of elements to determine alignment widths.
        /// </summary>
        public abstract IAttributeSequenceItemWidthComputer CreateWidthComputer();

        /// <summary>
        /// Computes the number of attributes in this list of names that we can handle.
        /// </summary>
        /// <param name="elementAttrNames">Attribute names</param>
        /// <returns>Number handled</returns>
        public int CountHandledAttributes(IEnumerable<string> elementAttrNames) => elementAttrNames.Count();
    }
}
