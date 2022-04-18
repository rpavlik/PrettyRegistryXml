// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;
using static MoreLinq.Extensions.GroupAdjacentExtension;


namespace PrettyRegistryXml.Core
{
    using Extensions;

    /// <summary>
    /// The base of your project-specific formatting class.
    /// </summary>
    /// <remarks>
    /// This class should already provide many if not all of the "primitives" you need to perform the desired formatting.
    /// Your derived class (specifically the override of <see cref="WriteElement(XmlWriter, XElement)"/>)
    /// is responsible for the project-specific "policy" that controls the output.
    /// </remarks>
    public abstract class XmlFormatterBase
    {
        /// <value>Width (in spaces) of a single indent level</value>
        public abstract int IndentLevelWidth { get; }

        /// <value>The string (probably several spaces) to use for one indent level.</value>
        public virtual string IndentChars { get => FormatterUtilities.MakeSpaces(IndentLevelWidth); }

        /// <summary>
        /// Return the indentation we'd expect from the nesting level (number of ancestors) of <paramref name="node"/>.
        /// </summary>
        /// <param name="node">A node</param>
        /// <param name="levelAdjust">Optional adjustment to nesting level</param>
        public string MakeIndent(XNode node, int levelAdjust = 0)
        {
            var level = node.Ancestors().Count() + levelAdjust;
            return FormatterUtilities.MakeSpaces(level * IndentLevelWidth);
        }

        /// <summary>
        /// Compute how much our indent level should differ from expected for a given node.
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>A signed integer</returns>
        public virtual int ComputeLevelAdjust(XNode node) => 0;

        /// <summary>
        /// Return the indentation we'd expect from the nesting level (number of ancestors) of <paramref name="node"/>
        /// with adjustments based on <see cref="ComputeLevelAdjust(XNode)"/>.
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>A string of whitespace</returns>
        public string MakeIndent(XNode node) => MakeIndent(node, ComputeLevelAdjust(node));

        private static bool IsWhitespace(XNode node)
        {
            if (node is XText text)
            {
                return string.IsNullOrWhiteSpace(text.Value);
            }
            return false;
        }

        /// <summary>
        /// Main entry point: process a root element into a formatted string.
        /// </summary>
        /// <param name="document">XML document</param>
        /// <returns>Formatted string</returns>
        public string Process(XDocument document)
        {
            var sb = new StringBuilder();

            // Hacky but hard to do otherwise
            if (document.Declaration != null)
            {
                sb.Append(document.Declaration.ToString());
                sb.Append(Environment.NewLine);
            }

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = this.IndentChars,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,

            };
            if (document.Root == null)
            {
                throw new InvalidOperationException("Document root cannot be null");
            }
            using (var writer = XmlWriter.Create(sb, settings))
            {
                WriteElement(writer, document.Root);
            }
            return Regex.Replace(sb.ToString(), @"\s*/>", "/>");
        }

        /// <summary>
        /// The main recursive function.
        /// </summary>
        /// <remarks>
        /// Please extend and handle your special formatting cases before falling back to calling this.
        /// </remarks>
        /// <param name="writer">The enclosing <see cref="XmlWriter"/></param>
        /// <param name="element">The element that needs to be written</param>
        protected virtual void WriteElement(XmlWriter writer, XElement element)
        {
            WriteStartElement(writer, element);
            WriteAttributes(writer, element);
            WriteNodes(writer, element.Nodes());
            WriteEndElement(writer, element);
        }

        /// <summary>
        /// Whether this whitespace node should be preserved. Can be overridden
        /// </summary>
        /// <param name="text">A whitespace text node</param>
        /// <returns>true if it should be preserved as-is (default)</returns>
        protected virtual bool PreserveWhitespace(XText text) => true;

        private void WriteAlignedElement(XmlWriter writer,
                                         XElement e,
                                         IAlignmentState alignment,
                                         StringBuilder sb)
        {
            WriteStartElement(writer, e);
            writer.Flush();
            var elementPadding = alignment.ComputeElementPaddingWidth(e);
            if (elementPadding > 0)
            {
                sb.Append(FormatterUtilities.MakeSpaces(elementPadding));
            }
            WriteAlignedAttrs(writer, e, alignment, sb);
            WriteNodes(writer, e.Nodes());
            WriteEndElement(writer, e);
        }

        /// <summary>
        /// Write attributes, aligned as indicated, to the writer with the associated StringBuilder.
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="e">Element whose attributes we should write</param>
        /// <param name="alignment">Your alignment state</param>
        /// <param name="sb">The StringBuilder that <paramref name="writer"/> writes to</param>
        public static void WriteAlignedAttrs(XmlWriter writer, XElement e, IAlignmentState alignment, StringBuilder sb)
            => WriteAlignedAttrs(writer,
                                 e,
                                 alignments: alignment.DetermineAlignment(from attr in e.Attributes() select attr.Name.ToString()),
                                 sb: sb);

        /// <summary>
        /// Write attributes, aligned as indicated, to the writer with the associated StringBuilder.
        /// </summary>
        /// <param name="writer">Your writer</param>
        /// <param name="e">Element whose attributes we should write</param>
        /// <param name="alignments">Array of alignments</param>
        /// <param name="sb">The StringBuilder that <paramref name="writer"/> writes to</param>
        public static void WriteAlignedAttrs(XmlWriter writer, XElement e, IEnumerable<AttributeAlignment> alignments, StringBuilder sb)
        {
            foreach (var alignment in alignments)
            {
                XAttribute? attr = alignment.IsPaddingOnly ? null : e.Attribute(alignment.Name);
                if (attr != null)
                {
                    writer.WriteAttributeString(alignment.Name, attr.Value);
                    writer.Flush();
                }
                alignment.AppendAttributePadding(attr, sb);
            }
        }

        /// <summary>
        /// Perform some writing to a custom <see cref="XmlWriter"/>, which is then written as "raw" to <paramref name="outerWriter"/>
        /// </summary>
        /// <remarks>
        /// If you need to manually modify the StringBuilder, be sure to do <c>writer.Flush();</c> first.
        /// </remarks>
        /// <param name="outerWriter">The current <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="wrapped">Your action to invoke on the wrapped writer</param>
        public static void WriteUsingWrappedWriter(XmlWriter outerWriter,
                                                   Action<XmlWriter, StringBuilder> wrapped)
                => WriteUsingWrappedWriter(outerWriter, settings: null, wrapped: wrapped);

        /// <summary>
        /// Perform some writing to a custom <see cref="XmlWriter"/>, which is then written as "raw" to <paramref name="outerWriter"/>
        /// </summary>
        /// <remarks>
        /// If you need to manually modify the StringBuilder, be sure to do <c>writer.Flush();</c> first.
        /// </remarks>
        /// <param name="outerWriter">The current <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="settings">The <see cref="XmlWriterSettings"/> which will be used (with slight modification) for the wrapped call</param>
        /// <param name="wrapped">Your action to invoke on the wrapped writer</param>
        public static void WriteUsingWrappedWriter(XmlWriter outerWriter,
                                                   XmlWriterSettings? settings,
                                                   Action<XmlWriter, StringBuilder> wrapped)
        {
            XmlWriterSettings mySettings = (settings == null) ? new XmlWriterSettings() : settings.Clone();

            mySettings.ConformanceLevel = ConformanceLevel.Fragment;
            mySettings.OmitXmlDeclaration = true;

            var sb = new StringBuilder();
            using (var newWriter = XmlWriter.Create(sb, mySettings))
            {
                wrapped(newWriter, sb);
            }
            var inner = sb.ToString();
            if (!string.IsNullOrEmpty(inner))
            {
                outerWriter.WriteRaw(inner);
            }
        }

        /// <summary>
        /// Write an element, keeping the opening and closing tags plus all its children on a single line.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        protected void WriteSingleLineElement(XmlWriter writer, XElement e)
        {
            WriteStartElement(writer, e);
            WriteAttributes(writer, e);
            var settings = writer.CloneOrCreateSettings();
            settings.Indent = false;
            WriteUsingWrappedWriter(writer, settings, (newWriter, sb) =>
            {
                WriteNodes(newWriter, e.Nodes());

            });
            WriteEndElement(writer, e);
        }

        // Suppressing because this is non-static to allow overriding.
#pragma warning disable CA1822
        /// <summary>
        /// Write all attributes of <paramref name="e"/> to <paramref name="writer"/>.
        /// </summary>
        /// <remarks>
        /// Not static despite analyzer suggestions, so it can be overridden.
        /// </remarks>
        /// <param name="writer">An XmlWriter in the correct state (has had <see cref="XmlFormatterBase.WriteStartElement(XmlWriter, XElement)"/> called)</param>
        /// <param name="e">An element that may have attributes.</param>
        protected void WriteAttributes(XmlWriter writer, XElement e)
        {
            foreach (var attr in e.Attributes())
            {
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
            }
        }
#pragma warning restore CA1822

        /// <summary>
        /// Write nodes, aligning element names and attributes of those that are elements.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="nodes">A collection of nodes</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        protected void WriteNodesWithEltsAligned(XmlWriter writer,
                                                 IEnumerable<XNode> nodes,
                                                 IAlignmentFinder alignmentFinder)
        {
            var nodeArray = nodes.ToArray();
            var elements = (from n in nodeArray
                            where n.NodeType == XmlNodeType.Element
                            select n as XElement).ToArray();
            if (elements.Length == 0)
            {
                // No elements to align
                WriteNodes(writer, nodeArray);
                return;
            }
            var alignment = alignmentFinder.FindAlignment(elements);

            WriteUsingWrappedWriter(writer, (newWriter, sb) =>
            {
                foreach (XNode node in nodeArray)
                {
                    if (node is XElement element)
                    {
                        WriteAlignedElement(newWriter, element, alignment, sb);
                    }
                    else
                    {
                        WriteNode(newWriter, node);
                    }
                }
            });
        }

        /// <summary>
        /// Write a node.
        /// </summary>
        /// <remark>
        /// Delegates to <see cref="WriteElement(XmlWriter, XElement)"/> for nodes that are elements.
        /// For nodes that are text with a whitespace value, checks <see cref="PreserveWhitespace(XText)"/> before writing.
        /// Otherwise just calls <see cref="XNode.WriteTo(XmlWriter)"/>.
        /// </remark>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="node">A node</param>
        protected void WriteNode(XmlWriter writer, XNode node)
        {
            if (node is XElement element)
            {
                // Call our recursive policy-owning method
                WriteElement(writer, element);
                return;
            }
            if (node is XText text && IsWhitespace(node))
            {
                if (!PreserveWhitespace(text))
                {
                    if (!text.Value.Contains(Environment.NewLine))
                    {
                        writer.WriteRaw(" ");
                    }
                    // early out here to not write this.
                    return;
                }

                // Possibly munge this, then write it.
                CleanWhitespaceNode(text).WriteTo(writer);
                return;
            }
            // Write everything that remains in the "normal" way.
            node.WriteTo(writer);
        }

        /// <summary>
        /// Write the provided nodes.
        /// </summary>
        /// <remarks>
        /// For <see cref="XElement"/> nodes, <see cref="WriteElement(XmlWriter, XElement)"/> will be called to recursively apply your custom formatting policy.
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="nodes">A collection of nodes</param>
        protected void WriteNodes(XmlWriter writer, IEnumerable<XNode> nodes)
        {
            foreach (XNode node in nodes)
            {
                WriteNode(writer, node);
            }
        }

        /// <summary>
        /// Write an element, and write its children aligning attributes across contiguous groups of elements that match <paramref name="groupingPredicate"/>.
        /// </summary>
        /// <remarks>
        /// This overload of "WriteElementWithAlignedChildAttrsInGroups" forwards to
        /// <see cref="WriteElementWithAlignedChildAttrsInGroups(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Predicate{XNode})"/>,
        /// which is the most generic of this collection.
        /// If you want more than one group, each with its own alignment, use
        /// <see cref="WriteElementWithAlignedChildAttrsInMultipleGroups{TKey}(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Func{XElement, TKey}, bool)"/>.
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="groupingPredicate">A predicate determining if a given element is one to align attributes for.</param>
        /// <param name="includeEmptyTextNodesBetween">If true (default), any whitespace-only <see cref="XText"/>
        /// between nodes that satisfy <paramref name="groupingPredicate"/> will not interrupt a group of aligning elements</param>
        protected void WriteElementWithAlignedChildAttrsInGroups(
            XmlWriter writer,
            XElement e,
            Predicate<XElement> groupingPredicate,
            bool includeEmptyTextNodesBetween = true)
            => WriteElementWithAlignedChildAttrsInGroups(writer,
                                                         e,
                                                         alignmentFinder: defaultAlignmentFinder,
                                                         groupingPredicate: groupingPredicate,
                                                         includeEmptyTextNodesBetween: includeEmptyTextNodesBetween);


        /// <summary>
        /// Write an element, and write its children aligning attributes across contiguous groups of elements that match <paramref name="groupingPredicate"/>.
        /// </summary>
        /// <remarks>
        /// If you want more than one group, each with its own alignment, use
        /// <see cref="WriteElementWithAlignedChildAttrsInMultipleGroups{TKey}(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Func{XElement, TKey}, bool)"/>
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        /// <param name="groupingPredicate">A predicate determining if a given element is one to align attributes for.</param>
        /// <param name="ignoreNodePredicate">A predicate identifying non-element nodes that should be ignored if between aligned elements</param>
        protected void WriteElementWithAlignedChildAttrsInGroups(
            XmlWriter writer,
            XElement e,
            IAlignmentFinder alignmentFinder,
            Predicate<XElement> groupingPredicate,
            Predicate<XNode> ignoreNodePredicate)
        {
            WriteStartElement(writer, e);
            WriteAttributes(writer, e);
            var grouped = e.Nodes().GroupAdjacent(n => n is XElement element ? groupingPredicate(element) : ignoreNodePredicate(n));

            foreach (var g in grouped)
            {
                if (g.Key)
                {
                    // These should be aligned.
                    WriteNodesWithEltsAligned(writer, g, alignmentFinder);
                }
                else
                {
                    // These are not aligned.
                    WriteNodes(writer, g);
                }
            }
            WriteEndElement(writer, e);
        }

        /// <summary>
        /// Write an element, and write its children aligning attributes across contiguous groups of elements that match <paramref name="groupingPredicate"/>.
        /// </summary>
        /// <remarks>
        /// This overload of "WriteElementWithAlignedChildAttrsInGroups" forwards to
        /// <see cref="WriteElementWithAlignedChildAttrsInGroups(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Predicate{XNode})"/>,
        /// which is the most generic of this collection.
        /// If you want more than one group, each with its own alignment, use
        /// <see cref="WriteElementWithAlignedChildAttrsInMultipleGroups{TKey}(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, Func{XElement, TKey}, bool)"/>
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        /// <param name="groupingPredicate">A predicate determining if a given node is one to align attributes for.</param>
        /// <param name="includeEmptyTextNodesBetween">If true (default), any whitespace-only <see cref="XText"/>
        /// between nodes that satisfy <paramref name="groupingPredicate"/> will not interrupt a group of aligning elements</param>
        protected void WriteElementWithAlignedChildAttrsInGroups(
            XmlWriter writer,
            XElement e,
            IAlignmentFinder alignmentFinder,
            Predicate<XElement> groupingPredicate,
            bool includeEmptyTextNodesBetween = true)
            => WriteElementWithAlignedChildAttrsInGroups(writer,
                                                         e,
                                                         alignmentFinder: alignmentFinder,
                                                         groupingPredicate: groupingPredicate,
                                                         ignoreNodePredicate: n => includeEmptyTextNodesBetween
                                                                                   && XmlUtilities.IsWhitespaceBetweenSelectedElements(n, groupingPredicate));

        /// <summary>
        /// Write an element, and write its children aligning attributes across (multiple) contiguous groups of elements that match <paramref name="alignmentPredicate"/>.
        /// </summary>
        /// <remarks>
        /// If you only have one type of group, use the simpler
        /// <see cref="WriteElementWithAlignedChildAttrsInGroups(XmlWriter, XElement, IAlignmentFinder, Predicate{XElement}, bool)"/>
        /// or one of its overloads
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        /// <param name="alignmentPredicate">A predicate determining if a given element is one to align attributes for.</param>
        /// <param name="groupingFunc">A function returning a value to group elements on. (This allows multiple groups, instead of just one with the predicate in related functions.)</param>
        /// <param name="includeEmptyTextNodesBetween">If true (default), any whitespace-only <see cref="XText"/>
        /// between nodes that satisfy <paramref name="alignmentPredicate"/> will not interrupt a group of aligning elements</param>
        protected void WriteElementWithAlignedChildAttrsInMultipleGroups<TKey>(
            XmlWriter writer,
            XElement e,
            IAlignmentFinder alignmentFinder,
            Predicate<XElement> alignmentPredicate,
            Func<XElement, TKey> groupingFunc,
            bool includeEmptyTextNodesBetween = true)
        {
            WriteStartElement(writer, e);
            WriteAttributes(writer, e);


            // This wraps the element alignment predicate and and groupingFunc,
            // to make a function that returns a bool (whether to align, using predicate) and the group key type (using groupingFunc).
            // Also handles the whole "whitespace in between elements" business.
            (bool ShouldAlign, TKey? GroupKey) completeGrouping(XNode n)
            {
                if (n is XElement element && alignmentPredicate(element))
                {
                    return (ShouldAlign: true, GroupKey: groupingFunc(element));
                }
                if (includeEmptyTextNodesBetween && XmlUtilities.IsWhitespaceBetweenSelectedElements(n, alignmentPredicate))
                {
                    // Empty text gets the group key of the previous node.
                    TKey? prevKey = n.PreviousNode is XElement prevElement ? groupingFunc(prevElement) : default;
                    return (ShouldAlign: true, GroupKey: prevKey);
                }
                return (ShouldAlign: false, GroupKey: default(TKey));
            }

            // Now, group the nodes, and iterate through the groups.
            var grouped = e.Nodes().GroupAdjacent(completeGrouping);
            foreach (var g in grouped)
            {
                if (g.Key.ShouldAlign)
                {
                    // These should be aligned.
                    WriteNodesWithEltsAligned(writer, g, alignmentFinder);
                }
                else
                {
                    // These are not aligned.
                    WriteNodes(writer, g);
                }
            }
            WriteEndElement(writer, e);
        }

        /// <summary>
        /// Write an element, and write its children aligning attributes across all of them.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        protected void WriteElementWithAlignedChildAttrs(XmlWriter writer,
                                                         XElement e,
                                                         IAlignmentFinder alignmentFinder)
        {

            WriteStartElement(writer, e);
            WriteAttributes(writer, e);
            WriteNodesWithEltsAligned(writer, e.Nodes(), alignmentFinder);
            WriteEndElement(writer, e);
        }

        private readonly IAlignmentFinder defaultAlignmentFinder = new SimpleAlignment();

        /// <summary>
        /// Write an element, and write its children aligning attributes across all of them, taking into consideration the width of the element name itself.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        protected void WriteElementWithAlignedChildElts(XmlWriter writer,
                                                        XElement e) => WriteElementWithAlignedChildElts(writer, e, defaultAlignmentFinder);

        /// <summary>
        /// Write an element, and write its children aligning attributes across all of them, taking into consideration the width of the element name itself.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="alignmentFinder">Your alignment finder</param>
        protected void WriteElementWithAlignedChildElts(XmlWriter writer,
                                                        XElement e,
                                                        IAlignmentFinder alignmentFinder)
        {

            WriteStartElement(writer, e);
            WriteAttributes(writer, e);
            WriteNodesWithEltsAligned(writer, e.Nodes(), alignmentFinder);
            WriteEndElement(writer, e);
        }


        /// <summary>
        /// Write an element, wrapping each attribute onto its own line, then writing its children.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="levelAdjust">Adjustment to indentation level that would be assumed from nesting level</param>
        protected void WriteElementWithAttrNewlines(XmlWriter writer,
                                                    XElement e,
                                                    int levelAdjust = 0)
        {

            WriteUsingWrappedWriter(writer, (newWriter, sb) =>
            {
                WriteStartElement(newWriter, e);
                newWriter.Flush();
                foreach (var attr in e.Attributes())
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(MakeIndent(e, levelAdjust + 1));
                    newWriter.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
                    newWriter.Flush();
                }

                // Now write children
                WriteNodes(newWriter, e.Nodes());
                WriteEndElement(newWriter, e);
            });
        }

        /// <summary>
        /// Wraps <see cref="XmlWriter.WriteStartElement(string)"/>.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="element">An element</param>
        protected virtual void WriteStartElement(XmlWriter writer, XElement element)
        {
            writer.WriteStartElement(element.Name.LocalName);
        }

        /// <summary>
        /// Wraps <see cref="XmlWriter.WriteEndElement"/>.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="element">An element</param>
        protected virtual void WriteEndElement(XmlWriter writer, XElement element)
        {
            writer.WriteEndElement();
        }

        /// <summary>
        /// Allows use of a modified version of a whitespace-only node.
        /// </summary>
        /// <param name="whitespaceText">A whitespace-only node</param>
        /// <returns>A whitespace-only node, possibly a new one, possibly the same as the input</returns>
        protected virtual XText CleanWhitespaceNode(XText whitespaceText)
        {
            return whitespaceText;
        }
    }
}
