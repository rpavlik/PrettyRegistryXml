using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System;

namespace pretty_registry
{
    public class XmlFormatter : XmlFormatterBase
    {
        private readonly HashSet<string> singleLineContainers = new HashSet<string> { "member", "param", "proto" };
        private readonly Predicate<XElement> singleLinePredicate;

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

        // This is the recursive part
        protected override void WriteElement(XmlWriter writer, XElement e)
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
                WriteStartElementAndAttributes(writer, e);
                foreach (var node in e.Nodes())
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
                writer.WriteEndElement();
            }

        }

        void WriteElementWithAlignedChildAttrs(XmlWriter writer, XElement e)
        {
            WriteStartElementAndAttributes(writer, e);
            WriteNodesWithEltAlignedAttrs(writer, e.Nodes());
            writer.WriteEndElement();
        }
    }
}
