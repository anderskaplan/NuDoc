namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.IO;

    public class SlashdocSummaryHtmlFormatter
    {
        private static readonly char[] Whitespace = new[] { ' ', '\n', '\r', '\t' };

        private IAssemblyReflector _assemblyReflector;
        ILanguageSignatureProvider _language;

        public SlashdocSummaryHtmlFormatter(IAssemblyReflector assemblyReflector, ILanguageSignatureProvider language)
        {
            if (assemblyReflector == null) throw new ArgumentNullException("assemblyReflector");
            if (language == null) throw new ArgumentNullException("language");

            _assemblyReflector = assemblyReflector;
            _language = language;
        }

        public string FormatSummary(string xmlDescription)
        {
            if (xmlDescription == null)
            {
                return string.Empty;
            }

            var xmlReaderSettings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            var sb = new StringBuilder();
            using (var xmlReader = XmlReader.Create(new StringReader(xmlDescription), xmlReaderSettings))
            {
                int summaryNestLevel = 0;
                while (xmlReader.Read())
                {
                    HandleNode(ref summaryNestLevel, sb, xmlReader);
                }
            }

            return RemoveWhitespace(sb.ToString());
        }

        private void HandleNode(ref int summaryNestLevel, StringBuilder sb, XmlReader xmlReader)
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (!xmlReader.IsEmptyElement &&
                        xmlReader.Name.Equals("summary"))
                    {
                        summaryNestLevel++;
                    }
                    else if (summaryNestLevel > 0)
                    {
                        if (xmlReader.Name.Equals("c") || xmlReader.Name.Equals("code"))
                        {
                            sb.Append("<code>");
                        }
                        else if (xmlReader.Name.Equals("para"))
                        {
                            sb.Append("<p>");
                        }
                        else if (xmlReader.Name.Equals("see") || xmlReader.Name.Equals("seealso"))
                        {
                            sb.Append(CreateFragmentLinkForType(xmlReader.GetAttribute("cref")));
                        }
                        else if (xmlReader.Name.Equals("paramref") || xmlReader.Name.Equals("typeparamref"))
                        {
                            sb.Append(XmlEscape(xmlReader.GetAttribute("name")));
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    if (xmlReader.Name.Equals("summary"))
                    {
                        summaryNestLevel--;
                    }
                    else if (summaryNestLevel > 0)
                    {
                        if (xmlReader.Name.Equals("c") || xmlReader.Name.Equals("code"))
                        {
                            sb.Append("</code>");
                        }
                        else if (xmlReader.Name.Equals("para"))
                        {
                            sb.Append("</p>");
                        }
                    }
                    break;

                case XmlNodeType.Text:
                    if (summaryNestLevel > 0)
                    {
                        sb.Append(XmlEscape(xmlReader.Value));
                    }
                    break;
            }
        }

        private string CreateFragmentLinkForType(string cref)
        {
            if (string.IsNullOrWhiteSpace(cref))
            {
                return string.Empty;
            }

            var typeName = SlashdocIdentifierProvider.GetTypeName(cref);
            if (typeName == null)
            {
                return string.Empty;
            }

            var type = _assemblyReflector.LookupType(typeName);
            if (type != null)
            {
                return string.Format("<a href=\"#{0}\">{1}</a>", XmlEscape(_language.GetDisplayName(type)), XmlEscape(_language.GetShortDisplayName(type)));
            }
            else
            {
                return XmlEscape(typeName);
            }
        }

        private static string XmlEscape(string value)
        {
            return System.Security.SecurityElement.Escape(value);
        }

        private static string RemoveWhitespace(string s)
        {
            return string.Join(" ", s.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
