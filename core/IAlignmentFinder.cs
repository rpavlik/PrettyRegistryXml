// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Xml.Linq;


namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Interface to conceal both simple and more complex ways of aligning attributes.
    /// </summary>
    public interface IAlignmentFinder
    {
        /// <summary>
        /// Compute alignment state for some collection of elements
        /// </summary>
        /// <param name="elements">A collection of elements</param>
        /// <returns><see cref="IAlignmentState"/> implementation, which functions somewhat like an array of alignments</returns>
        IAlignmentState FindAlignment(IEnumerable<XElement> elements);
    }
}
