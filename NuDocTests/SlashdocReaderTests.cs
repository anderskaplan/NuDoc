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

        private static SlashdocDictionary ReadSampleAssemblySlashdoc()
        {
            using (var stream = new FileStream(@"..\..\..\SampleAssembly\bin\Debug\SampleAssembly.xml", FileMode.Open, FileAccess.Read))
            {
                return SlashdocReader.Parse(stream);
            }
        }
    }
}
