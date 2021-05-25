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

            public static AttributeAlignment MakeUnaligned(string name) => new AttributeAlignment(name, 0);
            public static AttributeAlignment ReplaceWidth(AttributeAlignment old, int alignWidth) => new AttributeAlignment(old.Name, alignWidth);
            public static AttributeAlignment ReplaceWithUnaligned(AttributeAlignment old) => MakeUnaligned(old.Name);
        };
        private static (AttributeAlignment[], AttributeAlignment[]) FindAttributeAlignmentsInternal(IEnumerable<XElement> elements)
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
                                    select a.Name.LocalName).ToList();
            var knownNames = alignedAttrNames.ToHashSet();
            var aligned = from name in alignedAttrNames
                          select new AttributeAlignment(name, lengthDictionary[name]);
            var leftovers =
                from a in lengthDictionary
                where !knownNames.Contains(a.Key)
                select AttributeAlignment.MakeUnaligned(a.Key);
            return (aligned.ToArray(), leftovers.ToArray());
        }
        protected static AttributeAlignment[] FindAttributeAlignments(IEnumerable<XElement> elements)
        {
            var (aligned, leftovers) = FindAttributeAlignmentsInternal(elements);

            var result = aligned.ToList();
            // Don't align after the last attribute.
            result[result.Count - 1] = AttributeAlignment.ReplaceWithUnaligned(result[result.Count - 1]);

            // Add all remaining attributes, with no alignment.
            result.AddRange(leftovers);
            return result.ToArray();
        }
        protected static AttributeAlignment[] FindAttributeAlignments(IEnumerable<XElement> elements, IEnumerable<KeyValuePair<string, int>> extraWidth)
        {
            var (aligned, leftovers) = FindAttributeAlignmentsInternal(elements);
            var extra = extraWidth.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            for (int i = 0; i < aligned.Length; i++)
            {
                var name = aligned[i].Name;
                if (extra.ContainsKey(name))
                {
                    aligned[i] = new AttributeAlignment(name, aligned[i].AlignWidth + extra[name]);
                }
            }
            var lastIndex = aligned.Length - 1;
            if (!extra.ContainsKey(aligned[lastIndex].Name))
            {
                // Don't align after the last attribute, if we didn't explicitly mention it.
                aligned[lastIndex] = AttributeAlignment.ReplaceWithUnaligned(aligned[lastIndex]);
            }
            var result = aligned.ToList();
            result.AddRange(leftovers);
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
