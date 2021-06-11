// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Aligment for an entire element: element name and attributes.
    /// </summary>
    /// <remarks>
    /// Typically created with <see cref="ElementAlignment.FindElementAlignment(IEnumerable{XElement}, IDictionary{string, int}?)"/>.
    /// Wraps an integer for element name alignment, and an array of <see cref="AttributeAlignment"/>
    /// </remarks>
    public struct ElementAlignment
    {
        /// <summary>
        /// Alignment for the element name
        /// </summary>
        public int NameAlignment { get; init; }

        /// <summary>
        /// An array of alignments for attributes.
        /// </summary>
        public AttributeAlignment[] AttributeAlignments { get; init; }

        #region XElement-related methods

        /// <summary>
        /// Find the largest element name length
        /// </summary>
        /// <param name="elements">A collection of elements</param>
        /// <returns>Maximum width/alignment</returns>
        public static int FindNameAlignment(IEnumerable<XElement> elements) => (from el in elements
                                                                                let len = el.Name.LocalName.Length
                                                                                select len).Max();

        /// <summary>
        /// Compute an ElementAlignment for a collection of elements.
        /// </summary>
        /// <remarks>
        /// This wraps both <see cref="AttributeAlignment.FindAttributeAlignments(IEnumerable{XElement}, IDictionary{string, int}?)" />
        /// and <see cref="ElementAlignment.FindElementAlignment(IEnumerable{XElement}, IDictionary{string, int})" />
        /// </remarks>
        /// <param name="elements">A collection of elements</param>
        /// <param name="extraWidth">Optional dictionary of attribute name to additional width</param>
        /// <returns>Array of alignments</returns>
        public static ElementAlignment FindElementAlignment(IEnumerable<XElement> elements, IDictionary<string, int>? extraWidth = null)
        {
            var elts = elements.ToArray();
            var nameAlignment = FindNameAlignment(elts);
            var attrAligns = AttributeAlignment.FindAttributeAlignments(elts, extraWidth);
            return new ElementAlignment
            {
                NameAlignment = nameAlignment,
                AttributeAlignments = attrAligns,
            };
        }

        #endregion


        #region Other helpers
        /// <summary>
        /// Compute padding as appropriate for an element name.
        /// </summary>
        /// <param name="element">An element whose name has been written already</param>
        public int ComputeElementPaddingWidth(XElement element)
        {
            var len = element.Name.LocalName.Length;
            if (len < NameAlignment)
            {
                return NameAlignment - len;
            }
            return 0;
        }
        /// <summary>
        /// Append padding to a StringBuilder as appropriate for an element name.
        /// </summary>
        /// <param name="element">An element whose name has been written already</param>
        /// <param name="stringBuilder">Where to append the spaces, if required.</param>
        public void AppendElementNamePadding(XElement element, StringBuilder stringBuilder)
        {
            var padLen = ComputeElementPaddingWidth(element);
            if (padLen > 0)
            {
                stringBuilder.Append("".PadRight(padLen));
            }
        }

        #endregion

    }
}
