namespace NuDocTests
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;
    using NuDoc;
    using NUnit.Framework;

    [TestFixture]
    public class SlashdocProcessorTests
    {
        [Test]
        public void ShouldGenerateSlashdocForPublicTypesAndMembersOnly()
        {
            var fileName = @"slashdoc.xml";

            File.Delete(fileName);

            var assembly = new AssemblyReflector(@"../../../SampleAssembly/bin/Debug/SampleAssembly.dll", new ConsoleLogger());
            using (var slashdocStream = new FileStream(@"../../../SampleAssembly/bin/Debug/SampleAssembly.xml", FileMode.Open, FileAccess.Read))
            {
                SlashdocProcessor.WritePublicApiSlashdoc(assembly, slashdocStream, fileName);
            }

            using (var xmlReader = XmlReader.Create(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
            {
                var doc = new XPathDocument(xmlReader);
                var navigator = doc.CreateNavigator();
                Assert.That(navigator.SelectSingleNode("/doc"), Is.Not.Null);
                Assert.That(navigator.SelectSingleNode("/doc/assembly/name/text()").Value, Is.EqualTo("SampleAssembly"), "assembly name is preserved.");
                
                Assert.That(
                    InnerXmlWithoutWhitespace(navigator.SelectSingleNode("/doc/members/member[@name='T:SampleAssembly.Class1']")), 
                    Is.EqualTo("<summary>Aclass.</summary>"),
                    "slashdoc for the public type Class1 is preserved.");
                Assert.That(
                    InnerXmlWithoutWhitespace(navigator.SelectSingleNode("/doc/members/member[@name='M:SampleAssembly.Class1.DoSomething(System.DateTime)']")), 
                    Is.EqualTo("<summary>IfIcouldonlyrememberwhat.</summary><paramname=\"when\">Andwhen.</param>"),
                    "slashdoc for the public member Class1.DoSomething is preserved.");
                Assert.That(
                    navigator.SelectSingleNode("/doc/members/member[@name='M:SampleAssembly.Class1.PrivateMethod']"), 
                    Is.Null, 
                    "slashdoc for the private member Class1.PrivateMethod is NOT preserved.");

                Assert.That(
                    navigator.SelectSingleNode("/doc/members/member[@name='T:SampleAssembly.InternalClass']"), 
                    Is.Null, 
                    "slashdoc for the non-public type InternalClass is NOT preserved.");

                Assert.That(navigator.SelectSingleNode("/doc/members/member[@name='T:No.Such.Type']"), Is.Null);
            }
        }

        private static string InnerXmlWithoutWhitespace(XPathNavigator node)
        {
            if (node != null)
            {
                return string.Join("", node.InnerXml.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                return null;
            }
        }
    }
}
