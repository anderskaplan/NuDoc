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
    public class SlashdocProcessorTests
    {
        [Test]
        public void ShouldGenerateSlashdocForPublicTypesAndMembersOnly()
        {
            var fileName = @"slashdoc.xml";

            File.Delete(fileName);

            using (var assembly = new AssemblyReflector(@"NuDocTests.exe", new Mock<ILog>().Object))
            {
                using (var slashdocStream = new FileStream(@"NuDocTests.xml", FileMode.Open, FileAccess.Read))
                {
                    SlashdocProcessor.WritePublicApiSlashdoc(assembly, slashdocStream, fileName);
                }
            }

            using (var xmlReader = XmlReader.Create(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
            {
                var doc = new XPathDocument(xmlReader);
                var navigator = doc.CreateNavigator();
                Assert.That(navigator.SelectSingleNode("/doc"), Is.Not.Null);
                Assert.That(navigator.SelectSingleNode("/doc/assembly/name/text()").Value, Is.EqualTo("NuDocTests"), "assembly name is preserved.");
                
                Assert.That(
                    InnerXmlWithoutWhitespace(navigator.SelectSingleNode("/doc/members/member[@name='T:TestData.Xyz.Foo.SlashdocTestClass']")), 
                    Is.EqualTo("<summary>Aclass.</summary>"),
                    "slashdoc for the public type SlashdocTestClass is preserved.");
                Assert.That(
                    InnerXmlWithoutWhitespace(navigator.SelectSingleNode("/doc/members/member[@name='M:TestData.Xyz.Foo.SlashdocTestClass.DoSomething(System.DateTime)']")), 
                    Is.EqualTo("<summary>IfIcouldonlyrememberwhat.</summary><paramname=\"when\">Andwhen.</param>"),
                    "slashdoc for the public member SlashdocTestClass.DoSomething is preserved.");
                Assert.That(
                    navigator.SelectSingleNode("/doc/members/member[@name='M:TestData.Xyz.Foo.SlashdocTestClass.PrivateMethod']"), 
                    Is.Null, 
                    "slashdoc for the private member SlashdocTestClass.PrivateMethod is NOT preserved.");

                Assert.That(
                    navigator.SelectSingleNode("/doc/members/member[@name='T:TestData.Xyz.Foo.InternalClass']"), 
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
