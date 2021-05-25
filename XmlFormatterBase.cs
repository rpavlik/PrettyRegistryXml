using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace pretty_registry
{
    public abstract class XmlFormatterBase
    {

        protected static Dictionary<string, int> FindAttributeMaxLengths(IEnumerable<XElement> elements)
        {
            var q = from el in elements
                    from attr in el.Attributes()
                    group attr.Value.Length by attr.Name.LocalName into g
                    select (Name: g.Key, MaxLength: g.Max());
            return q.ToDictionary(p => p.Name, p => p.MaxLength);
        }
        protected static string[] FindAttributeOrder(IEnumerable<XElement> elements, Dictionary<string, int> maxLengths)
        {
            return elements.First().Attributes().Select(a => a.Name.LocalName).ToArray();
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
            return sb.ToString().Replace("\" />", "\"/>");
        }

        protected abstract void WriteElement(XmlWriter writer, XElement element);
        protected void WriteElementWithAlignedAttrs(XmlWriter writer,
                                          XElement e,
                                          Dictionary<string, int> maxWidths,
                                          string[] attrOrder,
                                          StringBuilder sb)
        {
            writer.WriteStartElement(e.Name.LocalName);
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
                    writer.Flush();

                    WriteSpaces(sb, maxAttrWidth - attr.Value.Length);
                }

            }
            writer.WriteEndElement();
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

        protected void WriteStartElementAndAttributes(XmlWriter writer, XElement e)
        {
            writer.WriteStartElement(e.Name.LocalName, e.Name.NamespaceName);
            foreach (var attr in e.Attributes())
            {
                writer.WriteAttributeString(attr.Name.LocalName, attr.Name.NamespaceName, attr.Value);
            }
        }

    }
}
