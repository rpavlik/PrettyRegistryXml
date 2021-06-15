// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Xml.Linq;
using System;

namespace PrettyRegistryXml.Core
{
    /// <summary>
    /// Assorted utilities acting on objects from System.Xml.Linq
    /// </summary>
    public static class XmlUtilities
    {
        /// <summary>
        /// See if this node is just whitespace.
        /// </summary>
        /// <param name="node">A non-null <see cref="XNode"/></param>
        /// <returns>true if the node is an <see cref="XText"/> with a whitespace value.</returns>
        public static bool NodeIsWhitespaceText(XNode node) => node is XText text
                                                               && text != null
                                                               && string.IsNullOrWhiteSpace(text.Value);

        /// <summary>
        /// See if the closest neighboring nodes are elements that pass the predicate
        /// </summary>
        /// <param name="node">A non-null <see cref="XNode"/></param>
        /// <param name="elementPredicate">A predicate on <see cref="XElement"/></param>
        /// <returns>
        /// true if this node's immediately preceding and following <see cref="XNode"/>
        /// are both <see cref="XElement"/> and pass <paramref name="elementPredicate"/>
        /// </returns>
        public static bool NeighboringNodesAreElementsMeetingPredicate(XNode node,
                                                                       Predicate<XElement> elementPredicate)
                => node.PreviousNode is XElement prevElement
                   && elementPredicate(prevElement)
                   && node.NextNode is XElement nextElement
                   && elementPredicate(nextElement);

        /// <summary>
        /// See if this node is whitespace, with the immediately previous and following nodes
        /// both <see cref="XElement"/> objects meeting the predicate
        /// </summary>
        /// <param name="node">A non-null <see cref="XNode"/></param>
        /// <param name="elementPredicate">A predicate on <see cref="XElement"/></param>
        /// <returns>
        /// true if this node is whitespace or a comment,
        /// and both the immediately preceding and following nodes are
        /// <see cref="XElement"/> and succeed in <paramref name="elementPredicate"/>
        /// </returns>
        /// <remarks>
        /// Intended for use with 
        /// <see cref="XmlFormatterBase.WriteElementWithAlignedChildAttrsInGroups(System.Xml.XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Predicate{XNode})"/>
        /// </remarks>
        public static bool IsWhitespaceBetweenSelectedElements(XNode node,
                                                               System.Predicate<XElement> elementPredicate)
               => NodeIsWhitespaceText(node)
                  && NeighboringNodesAreElementsMeetingPredicate(node, elementPredicate);


    }
}
