using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;
using static MoreLinq.Extensions.GroupAdjacentExtension;

namespace pretty_registry
{
    public abstract class XmlFormatterBase
    {

        //ValueTuple<string, int>
        public struct AttributeAlignment
        {
            public string Name;
            public int AlignWidth;

            public bool ShouldAlign
            {
                get => (AlignWidth > 0);
            }

            public int FullWidth
            {
                get
                {
                    if (!ShouldAlign) throw new InvalidOperationException("Not logical to access FullWidth when we should not align this attribute");
                    return $"{Name}=''".Length + AlignWidth + 1;
                }
            }
            public AttributeAlignment(string name, int alignWidth) => (Name, AlignWidth) = (name, alignWidth);
        };
        protected static AttributeAlignment[] FindAttributeAlignments(IEnumerable<XElement> elements)
        {
            var q = from el in elements
                    from attr in el.Attributes()
                    group attr.Value.Length by attr.Name.LocalName into g
                    // select new Tuple<string, int>(g.Key, g.Max());
                    select (Name: g.Key, MaxLength: g.Max());
            // group (attr, attr.Value.Length) by attr.Name.LocalName into g
            // select (Name: g.Key, MaxLength: g.Max(arg => arg.Length));
            var lengthDictionary = q.ToDictionary(arg => arg.Name, arg => arg.MaxLength);

            var alignedAttrNames = (from a in elements.First().Attributes()
                                    select a.Name.LocalName).ToList();
            var knownNames = alignedAttrNames.ToHashSet();
            var result = (from name in alignedAttrNames
                          select new AttributeAlignment(name, lengthDictionary[name])).ToList();

            // Don't align after the last attribute.
            result[result.Count - 1] = new AttributeAlignment(result[result.Count - 1].Name, 0);

            // Add all remaining attributes, with no alignment.
            result.AddRange(
                from a in lengthDictionary
                where !knownNames.Contains(a.Key)
                select new AttributeAlignment(a.Key, 0)
            );
            return result.ToArray();
        }
        protected static string MakeIndent(XElement element)
        {
            var level = element.Ancestors().Count() + 1;
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
        protected static void WriteSpaces(XmlWriter writer, int num)
        {
            if (num > 0)
            {
                writer.WriteRaw(MakeSpaces(num));
            }
        }

        static void WriteSpaces(StringBuilder sb, int num)
        {
            if (num > 0)
            {
                sb.Append(MakeSpaces(num));
            }
        }

        public string Process(XElement root)
        {
            var sb = new StringBuilder();

            var settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,

            };
            using (var writer = XmlWriter.Create(sb))
            {
                WriteElement(writer, root);
            }
            return sb.ToString().Replace(" />", "/>");
        }

        protected abstract void WriteElement(XmlWriter writer, XElement element);
        protected void WriteElementWithAlignedAttrs(XmlWriter writer,
                                                    XElement e,
                                                    AttributeAlignment[] alignments,
                                                    StringBuilder sb)
        {
            writer.WriteStartElement(e.Name.LocalName);
            foreach (var alignment in alignments)
            {
                var attr = e.Attribute(alignment.Name);
                if (alignment.ShouldAlign)
                {
                    if (attr == null)
                    {
                        WriteSpaces(sb, alignment.FullWidth);
                    }
                    else
                    {
                        writer.WriteAttributeString(alignment.Name, attr.Value);
                        writer.Flush();

                        WriteSpaces(sb, alignment.AlignWidth - attr.Value.Length);
                    }
                }
                else if (attr != null)
                {
                    // Shouldn't align this attribute, but we should handle it.
                    writer.WriteAttributeString(alignment.Name, attr.Value);
                }
            }
            WriteNodes(writer, e.Nodes());
            writer.WriteEndElement();
        }

        public delegate void WrappedWrite(XmlWriter writer, StringBuilder stringBuilder);
        protected static void WriteUsingWrappedWriter(XmlWriter outerWriter, XmlWriterSettings settings, WrappedWrite wrapped)
        {
            var sb = new StringBuilder();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.OmitXmlDeclaration = true;
            using (var newWriter = XmlWriter.Create(sb, settings))
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
            WriteStartElementAndAttributes(writer, e);
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };
            WriteUsingWrappedWriter(writer, settings, (newWriter, sb) =>
            {
                foreach (var n in e.Nodes())
                {
                    n.WriteTo(newWriter);
                }

            });
            writer.WriteEndElement();
        }

        protected void WriteStartElementAndAttributes(XmlWriter writer, XElement e)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            foreach (var attr in e.Attributes())
            {
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
            }
        }

        protected void WriteNodesWithEltAlignedAttrs(XmlWriter writer, IEnumerable<XNode> nodes)
        {
            var nodeArray = nodes.ToArray();
            var elements = (from n in nodeArray
                            where n.NodeType == XmlNodeType.Element
                            select n as XElement).ToArray();
            var alignments = FindAttributeAlignments(elements);
            if (alignments.Count() == 0)
            {
                // nothing to align here?
                WriteNodes(writer, nodeArray);
                return;
            }
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                NewLineOnAttributes = false,
                CloseOutput = false,
            };

            WriteUsingWrappedWriter(writer, settings, (newWriter, sb) =>
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

        protected void WriteElementWithSelectivelyAlignedChildAttrs(
            XmlWriter writer,
            XElement e,
            System.Predicate<XNode> groupingPredicate,
            bool includeEmptyTextNodesBetween = true)
        {
            WriteStartElementAndAttributes(writer, e);
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
                    WriteNodesWithEltAlignedAttrs(writer, g);
                }
                else
                {
                    // These are not aligned.
                    WriteNodes(writer, g);
                }
            }
            writer.WriteEndElement();
        }

        protected void WriteElementWithAlignedChildAttrs(XmlWriter writer, XElement e)
        {
            WriteStartElementAndAttributes(writer, e);
            WriteNodesWithEltAlignedAttrs(writer, e.Nodes());
            writer.WriteEndElement();
        }
    }
}
