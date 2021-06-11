// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Simplest alignment: The attributes of the element with most attributes are aligned, any leftovers aren't aligned.
    /// </summary>
    public class SimpleAlignment : IAlignmentFinder
    {

        private IDictionary<string, int>? extraWidth;


        /// <param name="extraWidth">Optional dictionary of attribute name to additional width</param>
        public SimpleAlignment(IDictionary<string, int>? extraWidth = null) => this.extraWidth = extraWidth;

        /// <summary>
        /// Find the simple alignment, delegating to
        /// <see cref="ElementAlignment.FindElementAlignment(IEnumerable{XElement}, IDictionary{string, int}?)"/>
        /// </summary>
        public IAlignmentState FindAlignment(IEnumerable<XElement> elements) => new State(ElementAlignment.FindElementAlignment(elements, extraWidth));

        private class State : IAlignmentState
        {
            private ElementAlignment alignment;

            internal State(ElementAlignment alignment_) { alignment = alignment_; }
            public IEnumerable<AttributeAlignment> DetermineAlignment(IEnumerable<string> attributeNames)
            {
                var nameSet = new HashSet<string>(from name in attributeNames select name.ToString());
                return from a in alignment.AttributeAlignments
                       where nameSet.Contains(a.Name.ToString()) || a.ShouldAlign
                       select a;
            }

            public int ComputeElementPaddingWidth(XElement element)
                => alignment.ComputeElementPaddingWidth(element);
        }
    }
}
