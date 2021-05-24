using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;

namespace pretty_registry
{

    public class XmlFormatter
    {
        private readonly HashSet<string> singleLineContainers = new HashSet<string> { "member", "param", "proto" };
        private readonly Predicate<XElement> singleLinePredicate;

        private static Dictionary<string, int> FindAttributeMaxLengths(IEnumerable<XElement> elements)
        {
            var q = from el in elements
                    from attr in el.Attributes()
                    group attr.Value.Length by attr.Name.LocalName into g
                    select (Name: g.Key, MaxLength: g.Max());
            return q.ToDictionary(p => p.Name, p => p.MaxLength);
        }
        private static string[] FindAttributeOrder(IEnumerable<XElement> elements, Dictionary<string, int> maxLengths)
        {
            return elements.First().Attributes().Select(a => a.Name.LocalName).ToArray();
        }

        private bool ShouldAlignChildAttributes(XElement e)
        {
            return e.Name.LocalName == "tags";
        }
        public XmlFormatter()
        {
            var defineCategory = new XAttribute("category", "define");
            // Predicate<XElement> isCategoryDefine = e => e.Attribute("category")
            singleLinePredicate = e =>
                singleLineContainers.Contains(e.Name.LocalName) ||
                (e.Name.LocalName == "type" && e.Parent != null && e.Parent.Name.LocalName == "types" && !IsCategoryDefineOrStruct(e));
        }
        static bool IsCategoryDefine(XElement element)
        {
            return element.Attributes().Where(a => a.Name.LocalName == "category" && a.Value == "define").Any();
        }
        static bool IsCategoryDefineOrStruct(XElement element)
        {
            return element
            .Attributes()
            .Where(a => a.Name.LocalName == "category")
            .Where(a => a.Value == "define" || a.Value == "struct")
            .Any();
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
            return sb.ToString().Replace("\" />", "\"/>");
        }

        // This is the recursive part
        void WriteElement(XmlWriter writer, XElement e)
        {
            if (singleLinePredicate(e))
            {
                WriteSingleLineElement(writer, e);
            }
            else if (ShouldAlignChildAttributes(e))
            {
                WriteElementWithAlignedChildAttrs(writer, e);
            }
            // else if (ShouldSelectivelyAlignChildAttributes(e))
            // {
            //     WriteElementWithAlignedChildAttrs(writer, e);
            // }
            else
            {
                WriteStartElement(writer, e);
                foreach (var node in e.Nodes())
                {
                    // Try to recurse if we can
                    XElement e2 = node as XElement;
                    if (e2 != null)
                    {
                        WriteElement(writer, e2);
                    }
                    else
                    {
                        node.WriteTo(writer);
                    }
                }
                writer.WriteEndElement();
            }

        }

        static string MakeIndent(XElement element)
        {
            var level = element.Ancestors().Count() + 1;
            return new string(' ', level * 4);
        }

        void WriteElementWithAlignedChildAttrs(XmlWriter writer, XElement e)
        {
            var maxWidths = FindAttributeMaxLengths(e.Elements());
            var attributeOrder = FindAttributeOrder(e.Elements(), maxWidths);

            WriteStartElement(writer, e);
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                NewLineOnAttributes = false,
                CloseOutput = false,
            };
            var sb = new StringBuilder();
            using (var newWriter = XmlWriter.Create(sb, settings))
            {
                newWriter.WriteRaw(MakeIndent(e));
                foreach (var n in e.Nodes())
                {
                    var childElt = n as XElement;
                    if (childElt != null)
                    {
                        WriteElementWithAlignedAttrs(newWriter, childElt, maxWidths, attributeOrder, sb);
                    }
                    else
                    {

                        n.WriteTo(newWriter);

                    }
                }
                // newWriter.WriteRaw(Environment.NewLine);

            }
            var inner = sb.ToString();

            if (inner.Length > 0)
            {
                writer.WriteRaw(inner);
            }
            writer.WriteEndElement();
        }
        void WriteElementWithAlignedAttrs(
            XmlWriter writer,
            XElement e,
            Dictionary<string, int> maxWidths,
            string[] attrOrder,
            StringBuilder sb)
        {
            //     Console.WriteLine(String.Join(",", attrOrder));
            //     Console.WriteLine(String.Join(",", e.Attributes().Select(a => a.Name.LocalName)));
            WriteStartElement(writer, e, includeAttributes: false);
            foreach (var attrName in attrOrder)
            {
                // Console.WriteLine(attrName);
                var maxAttrWidth = maxWidths.GetValueOrDefault(attrName);
                var attr = e.Attributes().Where(a => a.Name.LocalName == attrName).First();
                if (attr == null)
                {
                    var needWidth = $"{attrName}=''".Length + maxAttrWidth + 1;
                    WriteSpaces(sb, needWidth);
                }
                else
                {
                    writer.WriteAttributeString(attrName, attr.Value);
                    // writer.WriteStartAttribute(attrName);
                    // writer.WriteString(attr.Value);
                    // writer.WriteEndAttribute();
                    writer.Flush();

                    WriteSpaces(sb, maxAttrWidth - attr.Value.Length);
                }

            }
            writer.WriteEndElement();
        }

        static void WriteSpaces(XmlWriter writer, int num)
        {
            if (num > 0)
            {
                writer.WriteRaw(new string(' ', num));
            }
        }

        static void WriteSpaces(StringBuilder sb, int num)
        {
            if (num > 0)
            {
                sb.Append(new string(' ', num));
            }
        }

        void WriteSingleLineElement(XmlWriter writer, XElement e)
        {
            WriteStartElement(writer, e);
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
            };
            var sb = new StringBuilder();
            using (var newWriter = XmlWriter.Create(sb, settings))
            {
                foreach (var n in e.Nodes())
                {
                    n.WriteTo(newWriter);
                }
            }
            var inner = sb.ToString().Trim();
            if (inner.Length > 0)
            {
                writer.WriteRaw(
                    inner);
            }
            writer.WriteEndElement();
        }

        void WriteStartElement(XmlWriter writer, XElement e, bool includeAttributes = true)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            if (includeAttributes)
            {
                foreach (var attr in e.Attributes())
                {
                    writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
                }
            }
        }
    }
}
