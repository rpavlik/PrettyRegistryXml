// Copyright 2021-2022 Collabora, Ltd
//
// SPDX-License-Identifier: MIT
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PrettyRegistryXml.Core
{

    /// <summary>
    /// Provides a way to preserve leading "header" lines normally discarded by .NET XML parsing/writing.
    /// </summary>
    public class XmlRoundtripper
    {

        /// <summary>
        /// Parse an XML file, and also load it into an XmlRoundtripper.
        /// This overload assumes UTF-8
        /// </summary>
        /// <param name="filename">Path of an XML file to parse</param>
        /// <param name="encoding">File encoding for reading and writing</param>
        /// <param name="document">The document object to populate</param>
        /// <returns>An object you can use to restore the header lines when writing out your document again.</returns>
        public static XmlRoundtripper ParseAndLoad(string filename, Encoding encoding, out XDocument document)
        {
            using (var reader = new StreamReader(filename, encoding))
            {
                document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            }
            return new XmlRoundtripper(new FileStream(filename, FileMode.Open, FileAccess.Read), encoding);
        }

        /// <summary>
        /// Parse an XML file, and also load it into an XmlRoundtripper.
        /// This overload assumes UTF-8.
        /// </summary>
        /// <param name="filename">Path of an XML file to parse</param>
        /// <param name="document">The document object to populate</param>
        /// <returns>An object you can use to restore the header lines when writing out your document again.</returns>
        public static XmlRoundtripper ParseAndLoad(string filename, out XDocument document) => ParseAndLoad(filename, Encoding.UTF8, out document);

        /// <summary>
        /// Construct this object, storing the "header lines" (those that start with &lt;? or &lt;!) for later reuse.
        /// If you want to also parse the XML into an <see cref="XDocument"/>, see <see cref="ParseAndLoad(string, Encoding, out XDocument)"/>.
        /// </summary>
        /// <param name="stream">A stream to read an XML file from</param>
        /// <param name="encoding">The encoding to use when reading and writing</param>
        public XmlRoundtripper(Stream stream, Encoding encoding)
        {
            _encoding = encoding;
            _header = new(ReadLines(stream).TakeWhile(IsHeader));
        }

        /// <summary>
        /// Construct this object, storing the "header lines" (those that start with &lt;? or &lt;!) for later reuse.
        /// If you want to also parse the XML into an <see cref="XDocument"/>, see <see cref="ParseAndLoad(string, out XDocument)"/>.
        /// Uses UTF-8 by default.
        /// </summary>
        /// <param name="stream">A stream to read an XML file from</param>
        public XmlRoundtripper(Stream stream) : this(stream, Encoding.UTF8) { }

        /// <summary>
        /// Write a document to file, replacing the header lines with the ones from the original input.
        /// </summary>
        /// <param name="newDoc">The contents of the new document, after being formatted</param>
        /// <param name="output">The stream to write the adjusted output to</param>
        public void Write(string newDoc, Stream output)
        {
            var newDocStream = new MemoryStream(newDoc.Length);
            using var writer = new StreamWriter(newDocStream, _encoding);
            writer.WriteLine(newDoc);
            writer.Flush();
            // Move back to start.
            newDocStream.Position = 0;

            // Glue the stored header lines on the new lines excluding their header lines.
            var newLines = _header.Concat(ReadLines(newDocStream).SkipWhile(IsHeader));

            using var realWriter = new StreamWriter(output, _encoding);
            foreach (var line in newLines)
            {
                realWriter.WriteLine(line.TrimEnd());
            }
        }

        readonly Encoding _encoding;
        readonly List<string> _header;

        private static bool IsHeader(string line) => line.StartsWith("<?", StringComparison.InvariantCulture) || line.StartsWith("<!", StringComparison.InvariantCulture);

        private static IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
        {
            using var reader = new StreamReader(stream, encoding);
            string? line = reader.ReadLine();
            while (line != null)
            {
                yield return line;
                line = reader.ReadLine();
            }
        }

        private IEnumerable<string> ReadLines(Stream stream) => ReadLines(stream, _encoding);

    }
}
