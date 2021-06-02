// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Linq;
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
    /// Your derived class (specifically the override of <see cref="XmlFormatterBase.WriteElement(XmlWriter, XElement)"/>)
    /// is responsible for the project-specific "policy" that controls the output.
    /// </remarks>
    public abstract class XmlFormatterBase
    {

        /// <summary>
        /// Return the indentation we'd expect from the nesting level (number of ancestors) of <paramref name="element"/>.
        /// </summary>
        /// <remarks>
        /// Currently assumes that each level is 4 spaces.
        /// </remarks>
        /// <param name="element">An element</param>
        /// <param name="levelAdjust">Optional adjustment to nesting level</param>
        protected static string MakeIndent(XElement element, int levelAdjust = 0)
        {
            var level = element.Ancestors().Count() + levelAdjust;
            return new string(' ', level * 4);
        }


        private static bool IsWhitespace(XNode node)
        {
            if (node is XText text)
            {
                return text.Value.Trim().Length == 0;
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
            return sb.ToString().Replace(" />", "/>");
        }

        /// <value>The string (probably several spaces) to use for one indent level.</value>
        public abstract string IndentChars { get; }

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
            writer.WriteStartElement(element.Name.LocalName, element.Name.NamespaceName);
            WriteAttributes(writer, element);
            WriteNodes(writer, element.Nodes());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Whether this whitespace node should be preserved. Can be overridden
        /// </summary>
        /// <param name="text">A whitespace text node</param>
        /// <returns>true if it should be preserved as-is (default)</returns>
        protected virtual bool PreserveWhitespace(XText text)
        {
            return true;
        }

        private void WriteElementWithAlignedAttrs(XmlWriter writer,
                                                  XElement e,
                                                  AttributeAlignment[] alignments,
                                                  StringBuilder sb)
        {
            writer.WriteStartElement(e.Name.LocalName);
            WriteAlignedAttrs(writer, e, alignments, sb);
            WriteNodes(writer, e.Nodes());
            writer.WriteEndElement();
        }

        private void WriteElementWithAlignedAttrs(XmlWriter writer,
                                                  XElement e,
                                                  ElementAlignment alignment,
                                                  StringBuilder sb)
        {
            writer.WriteStartElement(e.Name.LocalName);
            writer.Flush();
            alignment.AppendElementNamePadding(e, sb);
            WriteAlignedAttrs(writer, e, alignment.AttributeAlignments, sb);
            WriteNodes(writer, e.Nodes());
            writer.WriteEndElement();
        }

        private static void WriteAlignedAttrs(XmlWriter writer, XElement e, AttributeAlignment[] alignments, StringBuilder sb)
        {
            foreach (var alignment in alignments)
            {
                XAttribute? attr = e.Attribute(alignment.Name);
                if (attr != null)
                {
                    writer.WriteAttributeString(alignment.Name, attr.Value);
                    writer.Flush();
                }
                alignment.AppendAttributePadding(attr, sb);
            }
        }

        /// <summary>
        /// A delegate type for <see cref="XmlFormatterBase.WriteUsingWrappedWriter(XmlWriter, XmlWriterSettings?, WrappedWrite)"/>
        /// </summary>
        /// <remarks>
        /// If you need to manually modify <paramref name="stringBuilder"/>, be sure to do <c>writer.Flush();</c> first.
        /// </remarks>
        /// <param name="writer">The new <see cref="XmlWriter"/> created for this call</param>
        /// <param name="stringBuilder">The StringBuilder that <paramref name="writer"/> is outputting to</param>
        public delegate void WrappedWrite(XmlWriter writer, StringBuilder stringBuilder);

        /// <summary>
        /// Perform some writing to a custom <see cref="XmlWriter"/>, which is then written as "raw" to <paramref name="outerWriter"/>
        /// </summary>
        /// <param name="outerWriter">The current <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="settings">The <see cref="XmlWriterSettings"/> which will be used (with slight modification) for the wrapped call</param>
        /// <param name="wrapped">Your action to invoke on the wrapped writer</param>
        /// <seealso cref="XmlFormatterBase.WrappedWrite"/>
        protected static void WriteUsingWrappedWriter(XmlWriter outerWriter, XmlWriterSettings? settings, WrappedWrite wrapped)
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
            if (inner.Length > 0)
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
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            var settings = writer.CloneOrCreateSettings();
            settings.Indent = false;
            WriteUsingWrappedWriter(writer, settings, (newWriter, sb) =>
            {
                WriteNodes(newWriter, e.Nodes());

            });
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write all attributes of <paramref name="e"/> to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">An XmlWriter in the correct state (has had <see cref="XmlWriter.WriteStartElement(string)"/> or similar called)</param>
        /// <param name="e">An element that may have attributes.</param>
        protected void WriteAttributes(XmlWriter writer, XElement e)
        {
            foreach (var attr in e.Attributes())
            {
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
            }
        }

        /// <summary>
        /// Write nodes, aligning element names and attributes of those that are elements.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="nodes">A collection of nodes</param>
        /// <param name="extraWidth">An optional dictionary of attribute name to additional width</param>
        protected void WriteNodesWithEltsAligned(XmlWriter writer, IEnumerable<XNode> nodes, IDictionary<string, int>? extraWidth = null)
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
            var alignment = ElementAlignment.FindElementAlignment(elements, extraWidth);

            WriteUsingWrappedWriter(writer, writer.Settings, (newWriter, sb) =>
            {
                foreach (XNode node in nodeArray)
                {
                    if (node is XElement element)
                    {
                        WriteElementWithAlignedAttrs(newWriter, element, alignment, sb);
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
        /// Delegates to <see cref="XmlFormatterBase.WriteElement(XmlWriter, XElement)"/> for nodes that are elements.
        /// For nodes that are text with a whitespace value, checks <see cref="XmlFormatterBase.PreserveWhitespace(XText)"/> before writing.
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
            if (node is XText text && IsWhitespace(node) && !PreserveWhitespace(text))
            {
                // early out here to not write this.
                return;
            }
            // Write everything that remains in the "normal" way.
            node.WriteTo(writer);
        }

        /// <summary>
        /// Write nodes, aligning the attributes of those that are elements.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="nodes">Some nodes</param>
        /// <param name="extraWidth">An optional dictionary of attribute name to additional width</param>
        protected void WriteNodesWithEltAlignedAttrs(XmlWriter writer,
                                                     IEnumerable<XNode> nodes,
                                                     IDictionary<string, int>? extraWidth = null)
        {
            var nodeArray = nodes.ToArray();
            var elements = (from n in nodeArray
                            where n.NodeType == XmlNodeType.Element
                            select n as XElement).ToArray();
            AttributeAlignment[] alignments = AttributeAlignment.FindAttributeAlignments(elements, extraWidth);
            if (alignments.Length == 0)
            {
                // nothing to align here?
                WriteNodes(writer, nodeArray);
                return;
            }

            WriteUsingWrappedWriter(writer, writer.Settings, (newWriter, sb) =>
            {
                foreach (XNode node in nodeArray)
                {
                    if (node is XElement element)
                    {
                        WriteElementWithAlignedAttrs(newWriter, element, alignments, sb);
                    }
                    else
                    {
                        WriteNode(newWriter, node);
                    }
                }
            });
        }

        /// <summary>
        /// Write the provided nodes.
        /// </summary>
        /// <remarks>
        /// For <see cref="XElement"/> nodes, <see cref="XmlFormatterBase.WriteElement(XmlWriter, XElement)"/> will be called to recursively apply your custom formatting policy.
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
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="groupingPredicate">A predicate determining if a given node is one to align attributes for.</param>
        /// <param name="includeEmptyTextNodesBetween">If true (default), any whitespace-only <see cref="XText"/>
        /// between nodes that satisfy <paramref name="groupingPredicate"/> will not interrupt a group of aligning elements</param>
        /// <param name="extraWidth">An optional dictionary of attribute name to additional width</param>
        protected void WriteElementWithAlignedChildAttrsInGroups(
            XmlWriter writer,
            XElement e,
            System.Predicate<XNode> groupingPredicate,
            bool includeEmptyTextNodesBetween = true,
            IDictionary<string, int>? extraWidth = null)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            var grouped = e.Nodes().GroupAdjacent(n =>
            {
                if (groupingPredicate(n))
                {
                    return true;
                }
                var text = n as XText;
                return includeEmptyTextNodesBetween
                       && n.NodeType == XmlNodeType.Text
                       && text != null
                       && (text.Value.Trim() == "")
                       && n.PreviousNode != null
                       && n.NextNode != null
                       && groupingPredicate(n.PreviousNode)
                       && groupingPredicate(n.NextNode);
            }
                );
            foreach (var g in grouped)
            {
                if (g.Key)
                {
                    // These should be aligned.
                    WriteNodesWithEltAlignedAttrs(writer, g, extraWidth);
                }
                else
                {
                    // These are not aligned.
                    WriteNodes(writer, g);
                }
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write an element, and write its children aligning attributes across all of them.
        /// </summary>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="extraWidth">An optional dictionary of attribute name to additional width</param>
        protected void WriteElementWithAlignedChildAttrs(XmlWriter writer, XElement e, Dictionary<string, int>? extraWidth = null)
        {

            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            WriteNodesWithEltAlignedAttrs(writer, e.Nodes(), extraWidth);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write an element, and write its children aligning attributes across all of them, taking into consideration the width of the element name itself.
        /// </summary>
        /// <remarks>
        /// Slightly more sophisticated than <see cref="XmlFormatterBase.WriteElementWithAlignedChildAttrs(XmlWriter, XElement, Dictionary{string, int}?)"/>
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="extraWidth">An optional dictionary of attribute name to additional width</param>
        protected void WriteElementWithAlignedChildElts(XmlWriter writer, XElement e, Dictionary<string, int>? extraWidth = null)
        {

            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            WriteNodesWithEltsAligned(writer, e.Nodes(), extraWidth);
            writer.WriteEndElement();
        }


        /// <summary>
        /// Write an element, wrapping each attribute onto its own line, then writing its children.
        /// </summary>
        /// <remarks>
        /// Slightly more sophisticated than <see cref="XmlFormatterBase.WriteElementWithAlignedChildAttrs(XmlWriter, XElement, Dictionary{string, int}?)"/>
        /// </remarks>
        /// <param name="writer">Your <see cref="XmlWriter"/> in the correct state</param>
        /// <param name="e">An element</param>
        /// <param name="levelAdjust">Adjustment to indentation level that would be assumed from nesting level</param>
        protected void WriteElementWithAttrNewlines(XmlWriter writer, XElement e, int levelAdjust = 0)
        {

            WriteUsingWrappedWriter(writer, writer.Settings, (newWriter, sb) =>
            {
                newWriter.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
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
                newWriter.WriteEndElement();
            });
        }
    }
}
