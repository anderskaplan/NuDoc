namespace NuDocTests
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using NuDoc;
    using NUnit.Framework;

    [TestFixture]
    public class ApiReferenceHtmlWriterTests
    {
        [Test]
        public void ShouldDescribeASetOfTypes()
        {
            var slashdoc = new SlashdocDictionary();
            slashdoc.SetXmlDescription("T:TestData.Xyz.Foo.TestClass", "<summary>Slashdoc summary for the TestClass class.</summary>");
            slashdoc.SetXmlDescription("M:TestData.Xyz.Foo.TestClass.#ctor(System.String)", "<summary>[string ctor]</summary>");
            slashdoc.SetXmlDescription("M:TestData.Xyz.Foo.TestClass.MethodReturningVoid", "<summary>[void method]</summary>");
            slashdoc.SetXmlDescription("F:TestData.Xyz.Foo.TestClass.x", "<summary>[field]</summary>");
            slashdoc.SetXmlDescription("E:TestData.Xyz.Foo.TestClass.AnEvent", "<summary>[event]</summary>");
            slashdoc.SetXmlDescription("T:TestData.Xyz.Foo.ITest", "<summary>Slashdoc summary for the ITest interface.</summary>");
            slashdoc.SetXmlDescription("F:TestData.Xyz.Foo.TestEnum.One", "<summary>[enum One]</summary>");

            // create a memory stream and write API documentation to it using an ApiReferenceHtmlWriter.
            var stream = new MemoryStream();
            using (var writer = new ApiReferenceHtmlWriter(stream, false, "Xyz", slashdoc, new CSharpSignatureProvider()))
            {
                writer.DescribeType(typeof(TestData.Xyz.Foo.TestClass));
                writer.DescribeType(typeof(TestData.Xyz.Foo.TestClass.NestedClass));
                writer.DescribeType(typeof(TestData.Xyz.Foo.ITest));
                writer.DescribeType(typeof(TestData.Xyz.Foo.TestStruct));
                writer.DescribeType(typeof(TestData.Xyz.Foo.TestEnum));
                writer.DescribeType(typeof(TestData.Xyz.Foo.TestGeneric<string, int>));
            }

            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            // rewind the stream and read it into an XPathDocument.
            stream.Seek(0, SeekOrigin.Begin);
            var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            var resolver = new XmlNamespaceManager(xmlReader.NameTable);
            resolver.AddNamespace("html", "http://www.w3.org/1999/xhtml");
            var doc = new XPathDocument(xmlReader);
            var navigator = doc.CreateNavigator();

            // check type info: TestClass
            var node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestClass']", resolver);
            var content = GetNodeContent(node);
            Assert.That(content.Contains("<h1>TestClass class</h1>"), "header");
            Assert.That(content.Contains("Slashdoc summary for the TestClass class."), "slashdoc summary");
            Assert.That(content.Contains("TestData.Xyz.Foo"), "namespace");
            Assert.That(content.Contains("class TestClass : System.ICloneable"), "signature. the access modifier is there because the class isn't public.");

            // constructors
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Constructors']", node, resolver);
            Assert.That(content.Contains("<td>TestClass(string xyz)</td>"), "constructor signature");
            Assert.That(content.Contains("[string ctor]"), "constructor slashdoc");
            Assert.That(!content.Contains("public"), "public access modifier is assumed and therefore not included");

            // properties
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Properties']", node, resolver);
            Assert.That(content.Contains("<td>int ReadOnlyProperty { get; }</td>"), "read-only property");
            Assert.That(content.Contains("<td>int ReadWriteProperty { get; set; }</td>"), "read/write property");
            Assert.That(!content.Contains("InternalProperty"), "internal property isn't listed");

            // methods
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Methods']", node, resolver);
            Assert.That(content.Contains("<td>object Clone()</td>"), "method signature");
            Assert.That(content.Contains("<td>void MethodReturningVoid()</td>"), "MethodReturningVoid");
            Assert.That(content.Contains("[void method]"), "method slashdoc");
            Assert.That(!content.Contains("~TestClass") && !content.Contains("Finalize()"), "the finalizer should not appear as a method.");
            Assert.That(!content.Contains("GetType()"), "trivial methods should be removed.");

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

            // check type info: TestClass.NestedClass
            content = GetSingleNodeContent("//html:div[@id='TestData.Xyz.Foo.TestClass.NestedClass']", navigator, resolver);
            Assert.That(content.Contains("<h1>TestClass.NestedClass class</h1>"), "header");
            Assert.That(content.Contains("TestData.Xyz.Foo"), "namespace");
            Assert.That(content.Contains("class TestClass.NestedClass"), "NestedClass signature");

            // check type info: ITest
            content = GetSingleNodeContent("//html:div[@id='TestData.Xyz.Foo.ITest']", navigator, resolver);
            Assert.That(content.Contains("<h1>ITest interface</h1>"), "header");
            Assert.That(content.Contains("Slashdoc summary for the ITest interface."), "slashdoc summary");

            // check type info: TestStruct
            node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestStruct']", resolver);
            content = GetNodeContent(node);
            Assert.That(content.Contains("<h1>TestStruct struct</h1>"), "header");
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Events']", node, resolver);
            Assert.That(content.Contains("<td>event System.EventHandler PublicEvent</td>"), "event signature");

            // check type info: TestEnum
            node = navigator.SelectSingleNode("//html:div[@id='TestData.Xyz.Foo.TestEnum']", resolver);
            content = GetNodeContent(node);
            Assert.That(content.Contains("<h1>TestEnum enum</h1>"), "header");
            content = GetSingleNodeContent(".//html:table[.//html:th/text()='Members']", node, resolver);
            Assert.That(content.Contains("<td>One</td>"), "member signature");
            Assert.That(content.Contains("<td>[enum One]</td>"), "member slashdoc");
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
