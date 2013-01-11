namespace NuDocTests
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using NuDoc;
    using NUnit.Framework;
    using Moq;

    [TestFixture]
    public class ApiReferenceHtmlWriterTests
    {
        private static readonly IAssemblyReflector DummyAssembly = new Mock<IAssemblyReflector>().Object;

        [Test]
        public void ShouldDescribeAClassAndItsMembers_CSharp()
        {
            var slashdoc = new SlashdocDictionary();
            slashdoc.SetXmlDescription("T:TestData.Xyz.Foo.TestClass", "<summary>Slashdoc summary for the TestClass class.</summary>");
            slashdoc.SetXmlDescription("M:TestData.Xyz.Foo.TestClass.#ctor(System.String)", "<summary>[string ctor]</summary>");
            slashdoc.SetXmlDescription("M:TestData.Xyz.Foo.TestClass.MethodReturningVoid", "<summary>[void method]</summary>");
            slashdoc.SetXmlDescription("F:TestData.Xyz.Foo.TestClass.x", "<summary>[field]</summary>");
            slashdoc.SetXmlDescription("E:TestData.Xyz.Foo.TestClass.AnEvent", "<summary>[event]</summary>");

            var stream = DescribeType(typeof(TestData.Xyz.Foo.TestClass), slashdoc);

            //var reader = new StreamReader(stream);
            //Console.WriteLine(reader.ReadToEnd());
            //stream.Seek(0, SeekOrigin.Begin);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            // check type info
            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestClass']", resolver);
            var content = GetNodeContent(node);
            Assert.That(content.Contains("<h2>TestClass class</h2>"), "header");
            Assert.That(content.Contains("Slashdoc summary for the TestClass class."), "slashdoc summary");
            Assert.That(content.Contains("TestData.Xyz.Foo"), "namespace");
            Assert.That(content.Contains("class TestClass : System.ICloneable"));

            // constructors
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Constructors']", node, resolver);
            Assert.That(content.Contains("<td>TestClass(string xyz)</td>"), "constructor signature");
            Assert.That(content.Contains("[string ctor]"), "constructor slashdoc");

            Assert.That(!content.Contains("public"), "The public access modifier is assumed and therefore not included");

            // properties
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Properties']", node, resolver);
            Assert.That(content.Contains("<td>int ReadOnlyProperty { get; }</td>"), "read-only property");
            Assert.That(content.Contains("<td>int ReadWriteProperty { get; set; }</td>"), "read/write property");

            Assert.That(!content.Contains("InternalProperty"), "Non-public property isn't included.");

            // methods
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Methods']", node, resolver);
            Assert.That(content.Contains("<td>object Clone()</td>"), "method signature");
            Assert.That(content.Contains("<td>void MethodReturningVoid()</td>"), "MethodReturningVoid");
            Assert.That(content.Contains("[void method]"), "method slashdoc");

            Assert.That(!content.Contains("~TestClass") && !content.Contains("Finalize()"), "The finalizer shall not appear as a method.");
            Assert.That(!content.Contains("GetType()"), "Trivial methods shall be removed.");

            // operators
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Operators']", node, resolver);
            Assert.That(content.Contains("<td>static TestClass operator !(TestClass t)</td>"), "method signature");

            // fields (including constants)
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Fields']", node, resolver);
            Assert.That(content.Contains("<td>int x</td>"), "field signature, x");
            Assert.That(content.Contains("<td>const bool y</td>"), "field signature, y");
            Assert.That(content.Contains("[field]"), "field slashdoc");

            // events
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Events']", node, resolver);
            Assert.That(content.Contains("<td>static event System.EventHandler AnEvent</td>"), "event signature");
            Assert.That(content.Contains("[event]"), "event slashdoc");
        }

        [Test]
        public void ShouldDescribeANestedClass_CSharp()
        {
            var slashdoc = new SlashdocDictionary();

            var stream = DescribeType(typeof(TestData.Xyz.Foo.TestClass.NestedClass), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var content = GetSingleNodeContent("//html:div[@id='TestData.Xyz.Foo.TestClass.NestedClass']", navigator, resolver);
            Assert.That(content.Contains("<h2>TestClass.NestedClass class</h2>"), "header");
            Assert.That(content.Contains("TestData.Xyz.Foo"), "namespace");
            Assert.That(content.Contains("class TestClass.NestedClass"), "NestedClass signature");
        }

        [Test]
        public void ShouldDescribeAnInterface_CSharp()
        {
            var slashdoc = new SlashdocDictionary();
            slashdoc.SetXmlDescription("T:TestData.Xyz.Foo.ITest", "<summary>Slashdoc summary for the ITest interface.</summary>");

            var stream = DescribeType(typeof(TestData.Xyz.Foo.ITest), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var content = GetSingleNodeContent("//html:div[@id='TestData.Xyz.Foo.ITest']", navigator, resolver);
            Assert.That(content.Contains("<h2>ITest interface</h2>"), "header");
            Assert.That(content.Contains("Slashdoc summary for the ITest interface."), "slashdoc summary");
        }

        [Test]
        public void ShouldDescribeAStruct_CSharp()
        {
            var slashdoc = new SlashdocDictionary();

            var stream = DescribeType(typeof(TestData.Xyz.Foo.TestStruct), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestStruct']", resolver);
            var content = GetNodeContent(node);
            Assert.That(content.Contains("<h2>TestStruct struct</h2>"), "header");
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Events']", node, resolver);
            Assert.That(content.Contains("<td>event System.EventHandler PublicEvent</td>"), "event signature");
        }

        [Test]
        public void ShouldDescribeAnEnum_CSharp()
        {
            var slashdoc = new SlashdocDictionary();
            slashdoc.SetXmlDescription("F:TestData.Xyz.Foo.TestEnum.One", "<summary>[enum One]</summary>");

            var stream = DescribeType(typeof(TestData.Xyz.Foo.TestEnum), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestEnum']", resolver);
            var content = GetNodeContent(node);
            Assert.That(content.Contains("<h2>TestEnum enum</h2>"), "header");
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Members']", node, resolver);
            Assert.That(content.Contains("<td>One</td>"), "member signature");
            Assert.That(content.Contains("<td>[enum One]</td>"), "member slashdoc");
        }

        [Test]
        public void ShouldDescribeAGenericClass_CSharp()
        {
            var slashdoc = new SlashdocDictionary();

            var stream = DescribeType(typeof(TestData.Xyz.Foo.TestGeneric<,>), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestGeneric<T, G>']", resolver);
            var content = GetNodeContent(node);
            Assert.That(content.Contains("<h2>TestGeneric&lt;T, G&gt; class</h2>"), "header");
        }

        [Test]
        public void ShouldXmlEscapeSlashdocSummaries()
        {
            var slashdoc = new SlashdocDictionary();
            slashdoc.SetXmlDescription("T:TestData.Xyz.Foo.ITest", "<summary>&lt;&lt;Hello&gt;&gt;</summary>");

            var stream = DescribeType(typeof(TestData.Xyz.Foo.ITest), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var content = GetSingleNodeContent("//html:div[@id='TestData.Xyz.Foo.ITest']", navigator, resolver);
            Assert.That(content.Contains("&lt;&lt;Hello&gt;&gt;"), "slashdoc summary");
        }

        [Test]
        public void ShouldReportMissingSlashdocSummariesWhenWarningsAreEnabled()
        {
            var mockLogger = new Mock<ILog>();
            var warningCount = 0;
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>())).Callback(() => warningCount++);
            mockLogger.Setup(x => x.LogError(It.IsAny<string>())).Callback(() => Assert.Fail("An error was logged."));

            DescribeTheSlashdocMappingTestClass(true, mockLogger.Object);

            Assert.That(warningCount, Is.EqualTo(4), "Missing summary warnings: type, constructor, property, event.");
        }

        [Test]
        public void ShouldNotReportMissingSlashdocSummariesWhenWarningsAreDisabled()
        {
            var mockLogger = new Mock<ILog>();
            mockLogger.Setup(x => x.LogWarning(It.IsAny<string>())).Callback(() => Assert.Fail("A warning was logged."));
            mockLogger.Setup(x => x.LogError(It.IsAny<string>())).Callback(() => Assert.Fail("An error was logged."));

            DescribeTheSlashdocMappingTestClass(false, mockLogger.Object);
        }

        [Test]
        public void ShouldNotIncludeTrivialMethods()
        {
            var slashdoc = new SlashdocDictionary();

            var stream = DescribeType(typeof(TestData.Xyz.Foo.PublicTestClass), slashdoc);

            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = CreateHtmlNamespaceResolver(xmlReader);
            var navigator = CreateXPathNavigator(xmlReader);

            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.PublicTestClass']", resolver);
            var content = GetNodeContent(node);
            Assert.That(!content.Contains("ToString()"), "the 'trivial' ToString method shouldn't be listed in the API reference.");
            Assert.That(!content.Contains("Equals(object"), "the 'trivial' ToString method shouldn't be listed in the API reference.");
            Assert.That(!content.Contains("GetHashCode()"), "the 'trivial' ToString method shouldn't be listed in the API reference.");
            Assert.That(content.Contains("ToString(int x)"), "the non-trivial ToString(int) method should be listed in the API reference.");
            Assert.That(content.Contains("Equals(PublicTestClass"), "the non-trivial Equals(PublicTestClass) method should be listed in the API reference.");
        }

        private static void DescribeTheSlashdocMappingTestClass(bool enableMissingSummaryWarnings, ILog logger)
        {
            var language = new CSharpSignatureProvider();
            using (var stream = new MemoryStream())
            {
                using (var writer = new ApiReferenceHtmlWriter(stream, false, "Xyz", new SlashdocDictionary(), language, logger))
                {
                    writer.EnableMissingSummaryWarnings = enableMissingSummaryWarnings;
                    writer.DescribeType(typeof(TestData.Xyz.Foo.SlashdocMappingTestClass), new SlashdocSummaryHtmlFormatter(DummyAssembly, language));
                }
            }
        }

        private static Stream DescribeType(Type type, SlashdocDictionary slashdoc)
        {
            var stream = new MemoryStream();
            var language = new CSharpSignatureProvider();
            using (var writer = new ApiReferenceHtmlWriter(stream, false, "Xyz", slashdoc, language, new Mock<ILog>().Object))
            {
                writer.DescribeType(type, new SlashdocSummaryHtmlFormatter(DummyAssembly, language));
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private static XmlNamespaceManager CreateHtmlNamespaceResolver(XmlReader xmlReader)
        {
            var resolver = new XmlNamespaceManager(xmlReader.NameTable);
            resolver.AddNamespace("html", "http://www.w3.org/1999/xhtml");
            return resolver;
        }

        private static XPathNavigator CreateXPathNavigator(XmlReader xmlReader)
        {
            var doc = new XPathDocument(xmlReader);
            return doc.CreateNavigator();
        }

        private static string GetSingleNodeContent(string xpath, XPathNavigator navigator, XmlNamespaceManager resolver)
        {
            var node = navigator.SelectSingleNode(xpath, resolver);
            return GetNodeContent(node);
        }

        private static string GetNodeContent(XPathNavigator node)
        {
            Assert.That(node, Is.Not.Null);
            return node.InnerXml.Replace(" xmlns=\"http://www.w3.org/1999/xhtml\">", ">");
        }
    }
}
