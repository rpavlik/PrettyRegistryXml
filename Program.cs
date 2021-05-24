using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace pretty_registry
{
    class Program
    {
        const string origFn = "/home/ryan/src/openxr/specification/registry/xr.xml";


        static void ReadLinqAndWrite()
        {
            XDocument document;
            var formatter = new XmlFormatter();
            using (var reader = new StreamReader(origFn))
            {
                document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }
            var result = formatter.Process(document.Root);
            using (var writer = new StreamWriter(origFn + ".result", false, Encoding.UTF8))
            {
                writer.WriteLine(result);
            }
        }
        static void Main(string[] args)
        {
            ReadLinqAndWrite();
        }
    }
}
