// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: BSL-1.0

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System;

using AttributeName = System.String;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// A structure storing the name of an attribute and a value width that it should be aligned/padded to.
    /// </summary>
    public struct AttributeAlignment
    {

        #region Properties
        /// <summary>
        /// The attribute name
        /// </summary>
        public AttributeName Name;


        /// <summary>
        /// Number of characters to allow for this attribute's string value. 0 is a sentinel that means "do not align"
        /// </summary>
        public int AlignWidth
        {
            get => _AlignWidth;
            set => _AlignWidth = CheckPossibleWidth(value);
        }

        /// <summary>
        /// Whether this attribute should be padded/aligned to a given width.
        /// </summary>
        public bool ShouldAlign
        {
            get => (AlignWidth > 0);
        }

        /// <summary>
        /// The width for the full attribute: name, equals sign, quotes, and value.
        /// Used when filling in for a missing attribute with blanks.
        /// </summary>
        public int FullWidth
        {
            get
            {
                if (!ShouldAlign) throw new InvalidOperationException("Makes no sense to access FullWidth when we should not align this attribute");
                return $"{Name}=\"\"".Length + AlignWidth + 1;
            }
        }
        #endregion

        #region Internal/Implementation

        private int _AlignWidth;

        /// <summary>
        /// Helper for throwing exceptions on invalid widths.
        /// </summary>
        /// <param name="value">User-proposed value for AlignWidth</param>
        /// <returns>value</returns>
        private static int CheckPossibleWidth(int value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("AlignWidth cannot be negative");
            return value;
        }
        #endregion


        #region String Conversion
        /// <summary>
        /// Format as a string.
        /// </summary>
        /// <returns>A string suitable for human reading during debugging/testing.</returns>
        public override string ToString() => ShouldAlign ? $"Alignment({Name} [{AlignWidth}])" : $"Unaligned({Name})";

        /// <summary>
        /// Format a collection of AttributeAlignment values as a string.
        /// </summary>
        /// <param name="alignments">A collection of AttributeAlignment values</param>
        /// <returns>A string suitable for human reading during debugging/testing.</returns>
        public static string FormatEnumerable(IEnumerable<AttributeAlignment> alignments) => string.Join(",", from a in alignments select a.ToString());
        #endregion

        #region Constructor/Factory Methods

        /// <summary>
        /// Normal constructor
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="alignWidth">Value width for alignment, or 0 to not align.</param>
        public AttributeAlignment(AttributeName name, int alignWidth) => (Name, _AlignWidth) = (name, CheckPossibleWidth(alignWidth));

        /// <summary>
        /// Make an AttributeAlignment that indicates the attribute should not be aligned.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Unaligned AttributeAlignment</returns>
        public static AttributeAlignment MakeUnaligned(AttributeName name) => new AttributeAlignment(name, 0);

        /// <summary>
        /// Make an AttributeAlignment with the same name but different width from the old one.
        /// </summary>
        /// <param name="old">Value to use name from</param>
        /// <param name="alignWidth">New width</param>
        /// <returns>AttributeAlignment with name from old.Name but with new width</returns>
        public static AttributeAlignment ReplaceWidth(AttributeAlignment old, int alignWidth) => new AttributeAlignment(old.Name, alignWidth);

        /// <summary>
        /// Make an AttributeAlignment with the same name but marked as unaligned.
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public static AttributeAlignment ReplaceWithUnaligned(AttributeAlignment old) => MakeUnaligned(old.Name);
        #endregion

        #region Other helpers

        public void AppendAttributePadding(XAttribute attribute, StringBuilder stringBuilder)
        {
            if (!ShouldAlign) return;
            if (attribute == null)
            {
                // Substituting for a full attribute
                stringBuilder.Append("".PadRight(FullWidth));
            }
            else
            {
                // Just right padding
                var len = ((string)attribute).Length;
                if (len < AlignWidth)
                {
                    stringBuilder.Append("".PadRight(AlignWidth - len));

                }
            }
        }

        #endregion

        #region XElement-related methods
        /// <summary>
        /// Compute an array of AttributeAlignment for a collection of elements.
        /// The element with the most attributes is used to extract the attributes to align and their order.
        /// The align width for each of those attributes is the maximum length of that attribute's value across all elements,
        /// except for the last attribute, which gets set as "do not align" if it is not mentioned in extraWidth.
        /// This prevents a lot of space before the end of the tag.
        /// Any other attributes mentioned in other elements are added to the end of the list as "do not align",
        /// so the resulting list contains every attribute name in the entire collection of elements.
        /// </summary>
        /// <param name="elements">A collection of elements</param>
        /// <param name="extraWidth">Optional dictionary of attribute name to additional width</param>
        /// <returns>Array of alignments</returns>
        public static AttributeAlignment[] FindAttributeAlignments(IEnumerable<XElement> elements, IDictionary<AttributeName, int> extraWidth = null)
        {
            var (aligned, leftovers) = FindAttributeAlignmentsAndLeftovers(elements);
            if (extraWidth != null)
            {
                // Adjust for extra width
                for (int i = 0; i < aligned.Length; i++)
                {
                    var name = aligned[i].Name;
                    if (extraWidth.ContainsKey(name))
                    {
                        aligned[i] = new AttributeAlignment(name, aligned[i].AlignWidth + extraWidth[name]);
                    }
                }
            }
            // Don't align after the last attribute, unless we explicitly explicitly mentioned it in extraWidth
            var lastIndex = aligned.Length - 1;
            if (extraWidth == null || !extraWidth.ContainsKey(aligned[lastIndex].Name))
            {
                aligned[lastIndex] = AttributeAlignment.ReplaceWithUnaligned(aligned[lastIndex]);
            }

            // Combine the aligned ones with unaligned leftovers.
            return aligned.Concat(from name in leftovers
                                  select AttributeAlignment.MakeUnaligned(name))
                                  .ToArray();
        }

        /// <summary>
        /// Internal, purely-functional helper function for finding attribute alignments.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>An array of attribute alignments with the max width, based on the element with the most attributes,
        /// and an array of all attribute names found in the collection that aren't in the first array.</returns>
        private static (AttributeAlignment[], AttributeName[]) FindAttributeAlignmentsAndLeftovers(IEnumerable<XElement> elements)
        {
            var q = from el in elements
                    from attr in el.Attributes()
                    group attr.Value.Length by attr.Name.LocalName into g
                    select (Name: g.Key, MaxLength: g.Max());

            var lengthDictionary = q.ToDictionary(arg => arg.Name, arg => arg.MaxLength);
            var eltWithMostAttributes = (from elt in elements
                                         orderby elt.Attributes().Count() descending
                                         select elt).First();
            var alignedAttrNames = (from a in eltWithMostAttributes.Attributes()
                                    select a.Name.LocalName).ToArray();
            var knownNames = alignedAttrNames.ToHashSet();
            var aligned = from name in alignedAttrNames
                          select new AttributeAlignment(name, lengthDictionary[name]);
            var leftovers =
                from a in lengthDictionary
                where !knownNames.Contains(a.Key)
                select a.Key;
            return (aligned.ToArray(), leftovers.ToArray());
        }
        #endregion
    }

    public static class ElementNameAlignment
    {

        #region XElement-related methods
        public static int FindNameAlignment(IEnumerable<XElement> elements)
        {
            return (from el in elements
                    let len = el.Name.LocalName.Length
                    select len).Max();
        }
        #endregion

        #region Other helpers
        public static void AppendNamePadding(int nameAlignment, XElement element, StringBuilder stringBuilder)
        {
            var len = element.Name.LocalName.Length;
            if (len < nameAlignment)
            {
                stringBuilder.Append("".PadRight(nameAlignment - len));
            }
        }

        #endregion
    }

    public struct ElementAlignment
    {
        public int NameAlignment;

        public AttributeAlignment[] AttributeAlignments;
        #region Other helpers
        public void AppendElementNamePadding(XElement element, StringBuilder stringBuilder)
        {
            var len = element.Name.LocalName.Length;
            if (len < NameAlignment)
            {
                stringBuilder.Append("".PadRight(NameAlignment - len));
            }
        }
        #endregion

        #region XElement-related methods
        /// <summary>
        /// Compute an ElementAlignment for a collection of elements.
        /// This wraps both FindNameAlignment and FindAttributeAlignments.
        /// </summary>
        /// <param name="elements">A collection of elements</param>
        /// <param name="extraWidth">Optional dictionary of attribute name to additional width</param>
        /// <returns>Array of alignments</returns>
        public static ElementAlignment FindElementAlignment(IEnumerable<XElement> elements, IDictionary<AttributeName, int> extraWidth = null)
        {
            var elts = elements.ToArray();
            var nameAlignment = ElementNameAlignment.FindNameAlignment(elts);
            var attrAligns = AttributeAlignment.FindAttributeAlignments(elts, extraWidth);
            return new ElementAlignment
            {
                NameAlignment = nameAlignment,
                AttributeAlignments = attrAligns,
            };
        }
        #endregion
    }
}
