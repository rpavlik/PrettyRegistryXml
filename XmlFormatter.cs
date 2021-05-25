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

        public XmlFormatter()
        {
            var defineCategory = new XAttribute("category", "define");
            singleLinePredicate = e =>
                singleLineContainers.Contains(e.Name.LocalName) ||
                (e.Name.LocalName == "type" && e.Parent != null && e.Parent.Name.LocalName == "types" && !IsCategoryDefineOrStruct(e));
        }

        static bool IsCategoryDefineOrStruct(XElement element)
        {
            return element
            .Attributes()
            .Where(a => a.Name.LocalName == "category")
            .Where(a => a.Value == "define" || a.Value == "struct")
            .Any();
        }

        private System.Predicate<XNode> isBitmask = node => {
            if (node.NodeType != XmlNodeType.Element)
            {
                return false;
            }
            var element = node as XElement;
            var attr = element.Attribute("category");
            return element.Name.LocalName == "type" && attr != null && attr.Value == "bitmask";
        };

        // This is the recursive part
        protected override void WriteElement(XmlWriter writer, XElement e)
        {
            if (singleLinePredicate(e))
            {
                WriteSingleLineElement(writer, e);
            }
            else if ((e.Name == "tags" || e.Name == "enums") && e.HasElements)
            {
                WriteElementWithAlignedChildAttrs(writer, e);
            }
            else if (e.Name == "types")
            {
                WriteElementWithSelectivelyAlignedChildAttrs(writer, e, isBitmask);
            }
            else
            {
                WriteStartElementAndAttributes(writer, e);
                WriteNodes(writer, e.Nodes());
                writer.WriteEndElement();
            }

        }
    }
}
