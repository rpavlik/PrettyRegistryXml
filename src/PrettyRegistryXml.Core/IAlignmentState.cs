// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Xml.Linq;


namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Interface wrapping the results of <see cref="IAlignmentFinder"/>
    /// </summary>
    public interface IAlignmentState
    {
        /// <summary>
        /// Get padding width as appropriate for an element name.
        /// </summary>
        /// <param name="element">An element whose name has been written already</param>
        /// <returns>the width to append</returns>
        int ComputeElementPaddingWidth(XElement element);

        /// <param name="attributeNames">The names of attributes for this element</param>
        /// <returns>A list of <see cref="AttributeAlignment"/> to either output the attribute or an equivalent empty space.</returns>
        IEnumerable<AttributeAlignment> DetermineAlignment(IEnumerable<string> attributeNames);
    }
}
