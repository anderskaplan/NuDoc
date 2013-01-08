namespace NuDocTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using NuDoc;
    using Moq;

    [TestFixture]
    public class AssemblyReflectorTests
    {
        [Test]
        public void ShouldLookTypesByName()
        {
            using (var assembly = new AssemblyReflector(@"NuDocTests.exe", new Mock<ILog>().Object))
            {
                Assert.That(assembly.LookupType("System.Guid"), Is.Null, "Lookup of a type which isn't in the assembly returns null.");

                var class1 = assembly.LookupType("TestData.Xyz.Foo.TestClass");
                Assert.That(class1, Is.Not.Null, "TestClass lookup succeeds.");

                var genericClass = assembly.LookupType("TestData.Xyz.Foo.TestGeneric`2");
                Assert.That(genericClass, Is.Not.Null, "Generic class lookup succeeds.");
            }
        }
    }
}
