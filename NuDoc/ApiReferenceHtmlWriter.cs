namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Produces API reference documentation on a HTML format; public types and members only.
    /// </summary>
    public class ApiReferenceHtmlWriter : IDisposable
    {
        private XmlWriter _writer;
        private SlashdocDictionary _slashdoc;
        private ILanguageSignatureProvider _language;

        public ApiReferenceHtmlWriter(string fileName, string title, SlashdocDictionary slashdoc, ILanguageSignatureProvider language)
            : this(new FileStream(fileName, FileMode.Create, FileAccess.Write), true, title, slashdoc, language)
        {
        }

        public ApiReferenceHtmlWriter(Stream stream, bool closeStream, string title, SlashdocDictionary slashdoc, ILanguageSignatureProvider language)
        {
            if (slashdoc == null) throw new ArgumentNullException("slashdoc");
            if (language == null) throw new ArgumentNullException("language");

            _slashdoc = slashdoc;
            _language = language;

            var writerSettings = new XmlWriterSettings { CloseOutput = closeStream };
            _writer = XmlWriter.Create(new StreamWriter(stream, new UTF8Encoding(false)), writerSettings);
            _writer.WriteStartDocument();
            _writer.WriteDocType("html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            _writer.WriteStartElement("html", "http://www.w3.org/1999/xhtml");

            _writer.WriteStartElement("head");
            WriteTextElement("title", title);
            _writer.WriteStartElement("style");
            _writer.WriteAttributeString("type", "text/css");
            WriteEmbeddedStyleSheet();
            _writer.WriteEndElement(); // style
            _writer.WriteEndElement(); // head

            _writer.WriteStartElement("body");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        virtual protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.WriteEndElement(); // body
                _writer.WriteEndElement(); // html
                _writer.Close();
                _writer = null;
            }
        }

        public void DescribeAssembly(IAssemblyReflector assembly)
        {
            var formatter = new SlashdocSummaryHtmlFormatter(assembly, _language);

            var title = string.Format("{0} public API reference", assembly.SimpleName);
            WriteTextElement("h1", title);

            WriteTypesOverviewTable(assembly, formatter);

            foreach (var type in assembly.Types
                .Where(t => ReflectionHelper.IsVisible(t))
                .OrderBy(t => _language.GetShortDisplayName(t)))
            {
                DescribeType(type, formatter);
            }
        }

        private void WriteEmbeddedStyleSheet()
        {
            _writer.WriteString("body { font-family: Arial, Helvetica, sans-serif; font-size: small; }");
            _writer.WriteString("h2 { margin-top: 30px; }");
            _writer.WriteString("table.descriptions { border-collapse: collapse; margin-bottom: 10px; }");
            _writer.WriteString("table.descriptions th, table.descriptions td { width: 400px; padding: 5px; border: 1px solid #E8E8E8; }");
            _writer.WriteString("table.descriptions th { background: #E8E8E8 }");
            _writer.WriteString("table.typeheader { border-collapse: collapse; border: none; width: 820px; }");
        }

        private void WriteTypesOverviewTable(IAssemblyReflector assembly, SlashdocSummaryHtmlFormatter formatter)
        {
            WriteDescriptionTableHeader("Types");

            foreach (var type in assembly.Types
                .Where(t => ReflectionHelper.IsVisible(t))
                .OrderBy(t => _language.GetDisplayName(t)))
            {
                _writer.WriteStartElement("tr");

                _writer.WriteStartElement("td");
                _writer.WriteStartElement("a");
                _writer.WriteAttributeString("href", "#" + _language.GetDisplayName(type));
                _writer.WriteString(_language.GetDisplayName(type));
                _writer.WriteEndElement(); // a
                _writer.WriteString(" " + _language.GetMetaTypeName(type));
                _writer.WriteEndElement(); // td

                _writer.WriteStartElement("td");
                var slashdocSummaryHtml = formatter.FormatSummary(_slashdoc.GetXmlDescription(SlashdocIdentifierProvider.GetId(type)));
                _writer.WriteRaw(slashdocSummaryHtml);
                _writer.WriteEndElement(); // td

                _writer.WriteEndElement(); // tr
            }

            WriteDescriptionTableFooter();
        }

        public void DescribeType(Type type, SlashdocSummaryHtmlFormatter formatter)
        {
            if (type == null) throw new ArgumentNullException("type");

            _writer.WriteStartElement("div");
            _writer.WriteAttributeString("id", _language.GetDisplayName(type));

            var displayName = _language.GetShortDisplayName(type);
            var metaType = _language.GetMetaTypeName(type);
            WriteTypeHeader(type, displayName, metaType, formatter);

            if (type.IsEnum)
            {
                var values = ReflectionHelper.GetEnumMembers(type)
                    .OrderBy(x => x.GetRawConstantValue());
                WriteSection("Members", values, (x) => x.Name, (x) => SlashdocIdentifierProvider.GetId(x), formatter);
            }
            else if (!HideMembers(type))
            {
                WriteTextElement("p", string.Format(CultureInfo.InvariantCulture, "The {0} {1} exposes the following members.", displayName, metaType));

                var constructors = ReflectionHelper.GetVisibleConstructors(type)
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Constructors", constructors, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);

                var properties = ReflectionHelper.GetVisibleProperties(type)
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Properties", properties, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);

                var methods = ReflectionHelper.GetVisibleMethods(type)
                    .Where(x => !ReflectionHelper.IsTrivialMethod(x))
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Methods", methods, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);

                var operators = ReflectionHelper.GetVisibleOperators(type)
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Operators", operators, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);

                var fields = ReflectionHelper.GetVisibleFields(type)
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Fields", fields, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);

                var events = ReflectionHelper.GetVisibleEvents(type)
                    .OrderBy(x => SlashdocIdentifierProvider.GetId(x));
                WriteSection("Events", events, (x) => _language.GetSignature(x), (x) => SlashdocIdentifierProvider.GetId(x), formatter);
            }

            _writer.WriteEndElement(); // div
        }

        private void WriteTypeHeader(Type type, string displayName, string metaType, SlashdocSummaryHtmlFormatter formatter)
        {
            WriteTextElement("h2", string.Format(CultureInfo.InvariantCulture, "{0} {1}", displayName, metaType));

            _writer.WriteStartElement("table");
            _writer.WriteAttributeString("class", "typeheader");
            _writer.WriteStartElement("tr");
            _writer.WriteStartElement("td");

            var slashdocSummaryHtml = formatter.FormatSummary(_slashdoc.GetXmlDescription(SlashdocIdentifierProvider.GetId(type)));
            _writer.WriteRaw(slashdocSummaryHtml);

            WriteInfo("Namespace", type.Namespace);
            WriteInfo("Signature", _language.GetSignature(type));

            _writer.WriteEndElement(); // td
            _writer.WriteEndElement(); // tr
            _writer.WriteEndElement(); // table
        }

        private static bool HideMembers(Type type)
        {
            return ReflectionHelper.IsDelegateType(type);
        }

        private void WriteInfo(string property, string value)
        {
            _writer.WriteStartElement("p");
            WriteTextElement("b", property + ": ");
            _writer.WriteString(value);
            _writer.WriteEndElement(); // p
        }

        private void WriteSection<T>(string sectionHeading, IEnumerable<T> items, Func<T, string> signatureProvider, Func<T, string> slashdocIdProvider, SlashdocSummaryHtmlFormatter formatter)
        {
            if (items.Count() > 0)
            {
                WriteDescriptionTableHeader(sectionHeading);
                foreach (var item in items)
                {
                    _writer.WriteStartElement("tr");
                    WriteTextElement("td", signatureProvider(item));
                    _writer.WriteStartElement("td");
                    var slashdocSummaryHtml = formatter.FormatSummary(_slashdoc.GetXmlDescription(slashdocIdProvider(item)));
                    _writer.WriteRaw(slashdocSummaryHtml);
                    _writer.WriteEndElement(); // td
                    _writer.WriteEndElement(); // tr
                }
                WriteDescriptionTableFooter();
            }
        }

        private void WriteDescriptionTableHeader(string title)
        {
            _writer.WriteStartElement("table");
            _writer.WriteAttributeString("class", "descriptions");
            _writer.WriteStartElement("tr");
            WriteTextElement("th", title);
            WriteTextElement("th", "Description");
            _writer.WriteEndElement(); // tr
        }

        private void WriteDescriptionTableFooter()
        {
            _writer.WriteEndElement(); // table
        }

        private void WriteTextElement(string elementName, string content)
        {
            _writer.WriteStartElement(elementName);
            _writer.WriteString(content);
            _writer.WriteEndElement(); // name
        }
    }
}
