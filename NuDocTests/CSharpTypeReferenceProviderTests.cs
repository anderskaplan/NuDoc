namespace NuDocTests
{
    using NuDoc;
    using NUnit.Framework;

    [TestFixture]
    public class CSharpTypeReferenceProviderTests
    {
        [Test]
        public void ShouldReplaceSystemTypesWithTheirCSharpBuiltInTypes()
        {
            // Built-In Types Table (C# Reference):
            // http://msdn.microsoft.com/en-us/library/ya5y69ds.aspx

            var typeReferencer = new CSharpTypeReferenceProvider();
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Boolean)), Is.EqualTo("bool"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Byte)), Is.EqualTo("byte"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.SByte)), Is.EqualTo("sbyte"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Char)), Is.EqualTo("char"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Decimal)), Is.EqualTo("decimal"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Double)), Is.EqualTo("double"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Single)), Is.EqualTo("float"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Int32)), Is.EqualTo("int"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.UInt32)), Is.EqualTo("uint"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Int64)), Is.EqualTo("long"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.UInt64)), Is.EqualTo("ulong"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Object)), Is.EqualTo("object"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.Int16)), Is.EqualTo("short"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.UInt16)), Is.EqualTo("ushort"));
            Assert.That(typeReferencer.GetTypeReference(typeof(System.String)), Is.EqualTo("string"));
        }

        [Test]
        public void ShouldShortenTypeNamesWithinTheSameNamespace()
        {
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(System.Xml.XmlAttribute)), Is.EqualTo("System.Xml.XmlAttribute"), "null context => full reference");
            Assert.That(new CSharpTypeReferenceProvider(typeof(System.Xml.XmlAttribute)).GetTypeReference(typeof(System.Xml.XmlAttribute)), Is.EqualTo("XmlAttribute"), "same namespace => reference without namespace");
            Assert.That(new CSharpTypeReferenceProvider(typeof(System.Action)).GetTypeReference(typeof(System.Xml.XmlAttribute)), Is.EqualTo("Xml.XmlAttribute"), "partially same namespace => reference with partial namespace");
        }

        [Test]
        public void ShouldShortenNestedTypeNamesWithGenericTypes()
        {
            var type = typeof(TestData.Xyz.Foo.TestGeneric<System.Xml.XmlAttribute, TestData.Xyz.Foo.TestClass.NestedClass>);
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<System.Xml.XmlAttribute, TestData.Xyz.Foo.TestClass.NestedClass>"), "null context => full reference");
            Assert.That(new CSharpTypeReferenceProvider(typeof(TestData.Xyz.Foo.TestClass)).GetTypeReference(type), Is.EqualTo("TestGeneric<System.Xml.XmlAttribute, NestedClass>"), "same namespace => reference without namespace");
            Assert.That(new CSharpTypeReferenceProvider(typeof(TestData.Xyz.Foo.PublicTestClass)).GetTypeReference(type), Is.EqualTo("TestGeneric<System.Xml.XmlAttribute, TestClass.NestedClass>"), "same namespace => reference without namespace");
            Assert.That(new CSharpTypeReferenceProvider(typeof(System.Xml.XmlAttribute)).GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<XmlAttribute, TestData.Xyz.Foo.TestClass.NestedClass>"), "same namespace => reference without namespace");
        }

        [Test]
        public void ShouldReferenceGenericTypes()
        {
            var type = typeof(TestData.Xyz.Foo.TestGeneric<,>);
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<T, G>"));
            type = typeof(TestData.Xyz.Foo.TestGeneric<TestData.Xyz.Foo.ITest, bool>);
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<TestData.Xyz.Foo.ITest, bool>"), "closed generic");
            type = typeof(TestData.Xyz.Foo.TestGeneric<System.Action<int>, bool>);
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<System.Action<int>, bool>"), "closed, nested generic");
        }

        [Test]
        public void ShouldReferenceArrayTypes()
        {
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(int[])), Is.EqualTo("int[]"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(int[][])), Is.EqualTo("int[][]"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(int[,])), Is.EqualTo("int[,]"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(int[, ,])), Is.EqualTo("int[, ,]"));
        }

        [Test]
        public void ShouldReferenceNullableTypesWithQuestionMarkNotation()
        {
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(TestData.Xyz.Foo.TestStruct?)), Is.EqualTo("TestData.Xyz.Foo.TestStruct?"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(bool?)), Is.EqualTo("bool?"));
        }

        [Test]
        public void ShouldReferenceNestedTypesWithDotNotation()
        {
            var type = typeof(TestData.Xyz.Foo.BirdsNest);
            var inner = typeof(TestData.Xyz.Foo.BirdsNest.First.Inner);
            var other = typeof(TestData.Xyz.Foo.TestClass);

            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(type), Is.EqualTo("TestData.Xyz.Foo.BirdsNest"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(inner), Is.EqualTo("TestData.Xyz.Foo.BirdsNest.First.Inner"));

            Assert.That(new CSharpTypeReferenceProvider(type).GetTypeReference(type), Is.EqualTo("BirdsNest"));
            Assert.That(new CSharpTypeReferenceProvider(type).GetTypeReference(inner), Is.EqualTo("First.Inner"));

            Assert.That(new CSharpTypeReferenceProvider(other).GetTypeReference(type), Is.EqualTo("BirdsNest"));
            Assert.That(new CSharpTypeReferenceProvider(other).GetTypeReference(inner), Is.EqualTo("BirdsNest.First.Inner"));
        }

        [Test]
        public void ShouldReferencePointerTypes()
        {
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(int*)), Is.EqualTo("int*"));
            Assert.That(new CSharpTypeReferenceProvider().GetTypeReference(typeof(void**)), Is.EqualTo("void**"));
        }
    }
}
