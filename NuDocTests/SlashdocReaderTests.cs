namespace NuDocTests
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using NuDoc;
    using NUnit.Framework;

    [TestFixture]
    public class SlashdocReaderTests
    {
        [Test]
        public void ShouldParseAValidSlashdocFile()
        {
            var slashdoc = ReadSampleAssemblySlashdoc();
            Assert.That(slashdoc.AssemblyName, Is.EqualTo("SampleAssembly"));
            Assert.That(slashdoc.GetXmlDescription("T:SampleAssembly.Class1"), Is.EqualTo("<summary>\n            A class.\n            </summary>"));
            Assert.That(slashdoc.GetXmlDescription("P:SampleAssembly.Class1.Foo"), Is.EqualTo("<summary>\n            An important property. <see cref=\"N:SampleAssembly\" /> Yes box allright.\n            </summary>"));
            Assert.That(slashdoc.GetXmlDescription("none-such"), Is.Null);
        }

        [Test]
        public void ShouldFailWhenReadingAnInvalidSlashdocFile()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("<doc>no closing tag"));
            Assert.That(() => SlashdocReader.Parse(stream), Throws.InstanceOf<XmlException>());
        }

        [Test]
        public void ShouldGetTextSummary()
        {
            Assert.That(SlashdocReader.GetTextSummary(null), Is.EqualTo(null));
            Assert.That(SlashdocReader.GetTextSummary("<far-out>dude</far-out>"), Is.EqualTo(string.Empty));
            Assert.That(SlashdocReader.GetTextSummary("<far-out><summary>dude</summary></far-out>"), Is.EqualTo("dude"));
            Assert.That(SlashdocReader.GetTextSummary("<summary>first</summary><summary><summary>second<summary/></summary></summary>"), Is.EqualTo("firstsecond"));
            Assert.That(SlashdocReader.GetTextSummary("irrelevant <summary/> irrelevant"), Is.EqualTo(string.Empty));
            Assert.That(SlashdocReader.GetTextSummary("<summary>&lt;hello&gt;</summary>"), Is.EqualTo("<hello>"));
        }

        [Test]
        public void ShouldIncludeTextWrappedInContainerElementsInTheSummary()
        {
            Assert.That(SlashdocReader.GetTextSummary("<summary><c>Hello</c>!</summary>"), Is.EqualTo("Hello!"));
            Assert.That(SlashdocReader.GetTextSummary("<summary><para><c>Hello</c>!</para></summary>"), Is.EqualTo("Hello!"));
            Assert.That(SlashdocReader.GetTextSummary("<summary><nest><another>hello</another></nest> content <xyz/></summary>"), Is.EqualTo("hello content"));
        }

        [Test]
        public void ShouldUnwrapReferencesInTheSummary()
        {
            Assert.That(SlashdocReader.GetTextSummary("<summary>Hello, <see cref=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));
            Assert.That(SlashdocReader.GetTextSummary("<summary>Hello, <seealso cref=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));
            Assert.That(SlashdocReader.GetTextSummary("<summary>Hello, <paramref name=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));
            Assert.That(SlashdocReader.GetTextSummary("<summary>Hello, <typeparamref name=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));

            Assert.That(SlashdocReader.GetTextSummary("<summary>Hello, <see />!</summary>"), Is.EqualTo("Hello, !"), "The parser doesn't choke when the attribute is missing.");
        }

        private static SlashdocDictionary ReadSampleAssemblySlashdoc()
        {
            return SlashdocReader.Parse(new FileStream(@"..\..\..\SampleAssembly\bin\Debug\SampleAssembly.xml", FileMode.Open, FileAccess.Read));
        }
    }
}
