namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Parses slashdoc xml files into slashdoc dictionaries.
    /// </summary>
    public sealed class SlashdocReader : IDisposable
    {
        private static readonly char[] Whitespace = new[] { ' ', '\n', '\r', '\t' };
        private Stream _stream;

        public SlashdocReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            _stream = stream;
        }

        public event Action<string> FoundAssemblyName;

        public event Action<KeyValuePair<string, string>> FoundMember;

        public static SlashdocDictionary Parse(Stream stream)
        {
            var slashdoc = new SlashdocDictionary();

            var instance = new SlashdocReader(stream);
            instance.FoundAssemblyName += (x) => slashdoc.AssemblyName = x;
            instance.FoundMember += (x) => slashdoc.SetXmlDescription(x.Key, x.Value);
            instance.Parse();

            return slashdoc;
        }

        public static string GetTextSummary(string xmlDescription)
        {
            if (xmlDescription == null)
            {
                return null;
            }

            var xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            using (var xmlReader = XmlReader.Create(new StringReader(xmlDescription), xmlReaderSettings))
            {
                if (xmlReader.ReadToFollowing("summary") && !xmlReader.IsEmptyElement)
                {
                    var sb = new StringBuilder();

                    xmlReader.Read();
                    while (xmlReader.NodeType != XmlNodeType.EndElement)
                    {
                        if (xmlReader.NodeType == XmlNodeType.Text)
                        {
                            sb.Append(xmlReader.Value);
                        }
                        xmlReader.Skip();
                    }

                    return RemoveWhitespace(sb.ToString());
                }
            }

            return string.Empty;
        }

        public void Parse()
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true, 
                CloseInput = false
            };

            using (var xmlReader = XmlReader.Create(_stream, xmlReaderSettings))
            {
                xmlReader.MoveToContent();
                if (xmlReader.NodeType == XmlNodeType.Element &&
                    string.Equals("doc", xmlReader.Name, StringComparison.Ordinal))
                {
                    // found the root node, "doc".

                    // read the assembly name
                    if (xmlReader.ReadToFollowing("assembly"))
                    {
                        if (xmlReader.ReadToDescendant("name"))
                        {
                            var handler = FoundAssemblyName;
                            if (handler != null) handler(xmlReader.ReadString());
                        }
                    }

                    // read members
                    if (xmlReader.ReadToFollowing("members"))
                    {
                        xmlReader.Read();
                        while (xmlReader.NodeType == XmlNodeType.Element &&
                            string.Equals("member", xmlReader.Name, StringComparison.Ordinal))
                        {
                            var name = xmlReader.GetAttribute("name");
                            var xmlDescription = xmlReader.ReadInnerXml();
                            var handler = FoundMember;
                            if (handler != null) handler(new KeyValuePair<string, string>(name, xmlDescription));
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static string RemoveWhitespace(string s)
        {
            return string.Join(" ", s.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries));
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }
        }
    }
}
