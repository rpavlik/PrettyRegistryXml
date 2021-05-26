// Copyright 2021 Collabora, Ltd
//
// SPDX-License-Identifier: BSL-1.0

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;
using static MoreLinq.Extensions.GroupAdjacentExtension;


namespace pretty_registry.core
{
    public abstract class XmlFormatterBase
    {
        protected static string MakeIndent(XElement element, int levelAdjust = 0)
        {
            var level = element.Ancestors().Count() + levelAdjust;
            return new string(' ', level * 4);
        }

        protected static string MakeSpaces(int num)
        {
            if (num <= 0)
            {
                return "";
            }
            return new string(' ', num);
        }

        static void WriteSpaces(StringBuilder sb, int num)
        {
            if (num > 0)
            {
                sb.Append(MakeSpaces(num));
            }
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
            sb.Append(document.Declaration.ToString());
            sb.Append(Environment.NewLine);

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = this.IndentChars,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,

            };
            using (var writer = XmlWriter.Create(sb, settings))
            {
                WriteElement(writer, document.Root);
            }
            return sb.ToString().Replace(" />", "/>");
        }

        public abstract string IndentChars { get; }

        /// <summary>
        /// The main recursive function.
        /// Please extend and handle your special formatting cases before falling back to calling this.
        /// </summary>
        /// <param name="writer">The enclosing XmlWriter</param>
        /// <param name="element">The element that needs to be written</param>
        protected virtual void WriteElement(XmlWriter writer, XElement element)
        {
            writer.WriteStartElement(element.Name.LocalName, element.Name.NamespaceName);
            WriteAttributes(writer, element);
            WriteNodes(writer, element.Nodes());
            writer.WriteEndElement();
        }

        protected void WriteElementWithAlignedAttrs(XmlWriter writer,
                                                    XElement e,
                                                    AttributeAlignment[] alignments,
                                                    StringBuilder sb)
        {
            writer.WriteStartElement(e.Name.LocalName);
            WriteAlignedAttrs(writer, e, alignments, sb);
            WriteNodes(writer, e.Nodes());
            writer.WriteEndElement();
        }

        protected void WriteElementWithAlignedAttrs(XmlWriter writer,
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
                var attr = e.Attribute(alignment.Name);
                if (attr != null)
                {
                    writer.WriteAttributeString(alignment.Name, attr.Value);
                    writer.Flush();
                }
                alignment.AppendAttributePadding(attr, sb);
            }
        }

        public delegate void WrappedWrite(XmlWriter writer, StringBuilder stringBuilder);
        protected static void WriteUsingWrappedWriter(XmlWriter outerWriter, XmlWriterSettings settings, WrappedWrite wrapped)
        {
            var sb = new StringBuilder();
            var mySettings = settings.Clone();
            mySettings.ConformanceLevel = ConformanceLevel.Fragment;
            mySettings.OmitXmlDeclaration = true;
            using (var newWriter = XmlWriter.Create(sb, mySettings))
            {
                wrapped(newWriter, sb);
            }
            var inner = sb.ToString();
            if (inner.Length > 0)
            {
                outerWriter.WriteRaw(
                    inner);
            }
        }

        protected void WriteSingleLineElement(XmlWriter writer, XElement e)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            var settings = writer.Settings.Clone();
            settings.Indent = false;
            WriteUsingWrappedWriter(writer, settings, (newWriter, sb) =>
            {
                foreach (var n in e.Nodes())
                {
                    n.WriteTo(newWriter);
                }

            });
            writer.WriteEndElement();
        }

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
        /// <param name="writer"></param>
        /// <param name="nodes"></param>
        /// <param name="extraWidths"></param>
        protected void WriteNodesWithEltsAligned(XmlWriter writer, IEnumerable<XNode> nodes, IDictionary<string, int> extraWidths = null)
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
            var alignment = ElementAlignment.FindElementAlignment(elements, extraWidths);

            WriteUsingWrappedWriter(writer, writer.Settings, (newWriter, sb) =>
            {
                foreach (var n in nodeArray)
                {
                    var childElt = n as XElement;
                    if (childElt != null)
                    {
                        WriteElementWithAlignedAttrs(newWriter, childElt, alignment, sb);
                    }
                    else
                    {
                        n.WriteTo(newWriter);
                    }
                }
            });
        }

        /// <summary>
        /// Write nodes, aligning the attributes of those that are elements.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="nodes"></param>
        /// <param name="extraWidths"></param>
        protected void WriteNodesWithEltAlignedAttrs(XmlWriter writer,
                                                     IEnumerable<XNode> nodes,
                                                     IDictionary<string, int> extraWidths = null)
        {
            var nodeArray = nodes.ToArray();
            var elements = (from n in nodeArray
                            where n.NodeType == XmlNodeType.Element
                            select n as XElement).ToArray();
            AttributeAlignment[] alignments = AttributeAlignment.FindAttributeAlignments(elements, extraWidths);
            if (alignments.Length == 0)
            {
                // nothing to align here?
                WriteNodes(writer, nodeArray);
                return;
            }

            WriteUsingWrappedWriter(writer, writer.Settings, (newWriter, sb) =>
            {
                foreach (var n in nodeArray)
                {
                    var childElt = n as XElement;
                    if (childElt != null)
                    {
                        WriteElementWithAlignedAttrs(newWriter, childElt, alignments, sb);
                    }
                    else
                    {
                        n.WriteTo(newWriter);
                    }
                }
            });
        }

        protected void WriteNodes(XmlWriter writer, IEnumerable<XNode> nodes)
        {
            foreach (var node in nodes)
            {
                // Try to recurse if we can
                if (node.NodeType == XmlNodeType.Element)
                {
                    WriteElement(writer, node as XElement);
                }
                else
                {
                    node.WriteTo(writer);
                }
            }
        }

        protected void WriteElementWithAlignedChildAttrsInGroups(
            XmlWriter writer,
            XElement e,
            System.Predicate<XNode> groupingPredicate,
            bool includeEmptyTextNodesBetween = true,
            IDictionary<string, int> extraWidth = null)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            var grouped = e.Nodes().GroupAdjacent(n =>
            {
                if (groupingPredicate(n))
                {
                    return true;
                }

                return includeEmptyTextNodesBetween
                       && n.NodeType == XmlNodeType.Text
                       && ((n as XText).Value.Trim() == "")
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
        /// <param name="writer"></param>
        /// <param name="e"></param>
        /// <param name="extraWidths"></param>
        protected void WriteElementWithAlignedChildAttrs(XmlWriter writer, XElement e, Dictionary<string, int> extraWidths = null)
        {

            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            WriteNodesWithEltAlignedAttrs(writer, e.Nodes(), extraWidths);
            writer.WriteEndElement();
        }
        protected void WriteElementWithAlignedChildElts(XmlWriter writer, XElement e, Dictionary<string, int> extraWidths = null)
        {

            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            WriteAttributes(writer, e);
            WriteNodesWithEltsAligned(writer, e.Nodes(), extraWidths);
            writer.WriteEndElement();
        }

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
