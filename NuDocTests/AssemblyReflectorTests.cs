namespace NuDocTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using NuDoc;

    [TestFixture]
    public class AssemblyReflectorTests
    {
        [Test]
        public void ShouldLookTypesByName()
        {
            var assembly = new AssemblyReflector(@"../../../SampleAssembly/bin/Debug/SampleAssembly.dll");
            
            Assert.That(assembly.LookupType("System.Guid"), Is.Null, "Lookup of a type which isn't in the assembly returns null.");

            var class1 = assembly.LookupType("SampleAssembly.Class1");
            Assert.That(class1, Is.Not.Null);

            var genericClass = assembly.LookupType("SampleAssembly.GenericClass`2");
            Assert.That(genericClass, Is.Not.Null);
        }
    }
}
