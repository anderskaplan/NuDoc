namespace NuDocTests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using NuDoc;
    using NUnit.Framework;

    [TestFixture]
    public class CSharpSignatureProviderTests
    {
        private const BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags PublicStatic = BindingFlags.Static | BindingFlags.Public;
        private const BindingFlags NonPublicStatic = BindingFlags.Static | BindingFlags.NonPublic;

        private CSharpSignatureProvider language = new CSharpSignatureProvider();

        #region Signatures for value types

        // -------------------------------------------------------------------------------------------------------------------
        // value types according to the C# 4.0 reference:
        // simple types, enum, struct, nullable.
        // we don't care about simple types nor nullables for type signatures.
        // -------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ShouldProvideTypeSignaturesForEnums()
        {
            var type = typeof(TestData.Xyz.Foo.TestEnum);
            Assert.That(language.GetSignature(type), Is.EqualTo("enum TestEnum"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForStructs()
        {
            var type = typeof(TestData.Xyz.Foo.TestStruct);
            Assert.That(language.GetSignature(type), Is.EqualTo("struct TestStruct : System.IFormattable"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForGenericStructs()
        {
            var type = typeof(TestData.Xyz.Foo.TestStructGeneric<>);
            Assert.That(language.GetSignature(type), Is.EqualTo("struct TestStructGeneric<T>"));
            type = typeof(TestData.Xyz.Foo.TestStructGeneric<int>);
            Assert.That(language.GetSignature(type), Is.EqualTo("struct TestStructGeneric<int>"), "closed generic");
            type = typeof(TestData.Xyz.Foo.TestStructGeneric<System.Action<int>>);
            Assert.That(language.GetSignature(type), Is.EqualTo("struct TestStructGeneric<System.Action<int>>"), "closed nested generic");
        }

        #endregion

        #region Signatures for reference types

        // -------------------------------------------------------------------------------------------------------------------
        // reference types according to the C# 4.0 reference:
        // class, interface, array, delegate.
        // we don't care about arrays for type signatures.
        // -------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ShouldProvideTypeSignaturesForClasses()
        {
            // class modifiers according to the C# 4.0 specification:
            // new, public, protected, internal, private, abstract, sealed, and static.
            // we ignore all except abstract and static.

            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.PublicTestClass)), Is.EqualTo("class PublicTestClass"), "public class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.InternalTestClass)), Is.EqualTo("class InternalTestClass"), "internal class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.AbstractTestClass)), Is.EqualTo("abstract class AbstractTestClass"), "abstract class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.SealedTestClass)), Is.EqualTo("class SealedTestClass"), "sealed class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.StaticTestClass)), Is.EqualTo("static class StaticTestClass"), "static class");
        }

        [Test]
        public void ShouldProvideTypeSignaturesForClassesWithInheritanceAndImplementingInterfaces()
        {
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.TestClass)), Is.EqualTo("class TestClass : System.ICloneable"), "implementing interface");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.SpecializedTestClass)), Is.EqualTo("class SpecializedTestClass : PublicTestClass"), "inheritance");
            Assert.That(language.GetSignature(typeof(System.Xml.XmlTextWriter)), Is.EqualTo("class XmlTextWriter : XmlWriter, IDisposable"), "inheritance + implementing interfaces");
        }

        [Test]
        public void ShouldProvideTypeSignaturesForGenericClasses()
        {
            var type = typeof(TestData.Xyz.Foo.TestGeneric<,>);
            Assert.That(language.GetSignature(type), Is.EqualTo("class TestGeneric<T, G>"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForInterfaces()
        {
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.ITest)), Is.EqualTo("interface ITest"));
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.ITest2)), Is.EqualTo("interface ITest2 : System.IDisposable"), "interface with inheritance");
        }

        [Test]
        public void ShouldProvideTypeSignaturesForGenericInterfaces()
        {
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.IGeneric<>)), Is.EqualTo("interface IGeneric<T>"));
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.IGeneric<int>)), Is.EqualTo("interface IGeneric<int>"), "closed generic");
        }

        [Test]
        public void ShouldLeaveOutModifiersOnInterfaceMembers()
        {
            var type = typeof(TestData.Xyz.Foo.ITest);
            Assert.That(language.GetSignature(type.GetMethod("Foo")), Is.EqualTo("void Foo(int count)")); // no public, no abstract
            Assert.That(language.GetSignature(type.GetProperty("Whatever")), Is.EqualTo("int Whatever { get; }"));
            Assert.That(language.GetSignature(type.GetEvent("Bang")), Is.EqualTo("event System.EventHandler Bang"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForDelegates()
        {
            var type = typeof(TestData.Xyz.Foo.Delegate1);
            Assert.That(language.GetSignature(type), Is.EqualTo("delegate int Delegate1(int x)"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForGenericDelegates()
        {
            var type = typeof(TestData.Xyz.Foo.GenericDelegate<,>);
            Assert.That(language.GetSignature(type), Is.EqualTo("delegate Y GenericDelegate<T, Y>(T x)"));
        }

        #endregion

        #region Member signatures

        // -------------------------------------------------------------------------------------------------------------------
        // class members according to the C# 4.0 reference:
        // constants, fields, methods, properties, indexers, events, operators, constructors, destructors, and (nested) types.
        // -------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ShouldProvideSignaturesForConstantMembers()
        {
            // const modifiers according to the C# 4.0 reference:
            // new, public, protected, internal, and private.
            // we ignore all of them.

            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetField("constField")), Is.EqualTo("const int constField"));
            Assert.That(language.GetSignature(type.GetField("internalConstField", NonPublicStatic)), Is.EqualTo("const int internalConstField"));
        }

        [Test]
        public void ShouldProvideSignaturesForFieldMembers()
        {
            // field modifiers according to the C# 4.0 reference:
            // new, public, protected, internal, private, static, readonly, and volatile.
            // we ignore all except static and readonly.

            // static, readonly
            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetField("staticField", PublicStatic)), Is.EqualTo("static int staticField"));
            Assert.That(language.GetSignature(type.GetField("readonlyField")), Is.EqualTo("readonly int readonlyField"));
            Assert.That(language.GetSignature(type.GetField("staticReadonlyField", PublicStatic)), Is.EqualTo("static readonly int staticReadonlyField"));

            // other
            Assert.That(language.GetSignature(type.GetField("publicField")), Is.EqualTo("int publicField"));
            Assert.That(language.GetSignature(type.GetField("protectedField", NonPublicInstance)), Is.EqualTo("int protectedField"));
            Assert.That(language.GetSignature(type.GetField("internalField", NonPublicInstance)), Is.EqualTo("int internalField"));
            Assert.That(language.GetSignature(type.GetField("privateField", NonPublicInstance)), Is.EqualTo("int privateField"));
        }


        [Test]
        public void ShouldProvideSignaturesForMethodMembers()
        {
            // member modifiers according to the C# 4.0 reference:
            // new, public, protected, internal, private, static, virtual, sealed, override, abstract, and extern.
            // we ignore all except static and abstract.

            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetMethod("PublicMethod")), Is.EqualTo("void PublicMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("ProtectedMethod", NonPublicInstance)), Is.EqualTo("void ProtectedMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("InternalMethod", NonPublicInstance)), Is.EqualTo("void InternalMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("PrivateMethod", NonPublicInstance)), Is.EqualTo("void PrivateMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("StaticMethod", PublicStatic)), Is.EqualTo("static void StaticMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("VirtualMethod")), Is.EqualTo("void VirtualMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("SealedMethod")), Is.EqualTo("void SealedMethod()"));
            Assert.That(language.GetSignature(type.GetMethod("AbstractMethod")), Is.EqualTo("abstract void AbstractMethod()"));
        }

        [Test]
        public void ShouldProvideSignaturesForGenericMethodMembers()
        {
            var type = typeof(TestData.Xyz.Foo.TestGeneric<,>);
            Assert.That(language.GetSignature(type.GetMethod("Foo")), Is.EqualTo("G Foo(T t)"));
            Assert.That(language.GetSignature(type.GetMethod("HalfOpen")), Is.EqualTo("TestGeneric<int, G> HalfOpen()"));
            Assert.That(language.GetSignature(type.GetMethod("HalfOpenParameter")), Is.EqualTo("void HalfOpenParameter(TestGeneric<int, G> parameter)"));

            var type2 = typeof(TestData.Xyz.Foo.TestClassWithGenericMethod);
            Assert.That(language.GetSignature(type2.GetMethod("Bar")), Is.EqualTo("void Bar<Q>(Q q)"));
        }

        [Test]
        public void ShouldProvideSignaturesForExtensionMethodMembers()
        {
            var type = typeof(TestData.Xyz.Foo.StaticTestClass);
            Assert.That(language.GetSignature(type.GetMethod("ExtensionMethod")), Is.EqualTo("static void ExtensionMethod(this InternalTestClass subject)"));
        }

        [Test]
        public void ShouldProvideSignaturesForPropertyMembers()
        {
            var type = typeof(TestData.Xyz.Foo.TestClass);
            Assert.That(language.GetSignature(type.GetProperty("ReadWriteProperty")), Is.EqualTo("int ReadWriteProperty { get; set; }"), "public get, set");
            Assert.That(language.GetSignature(type.GetProperty("ReadOnlyProperty")), Is.EqualTo("int ReadOnlyProperty { get; }"), "public get, no set");
            Assert.That(language.GetSignature(type.GetProperty("SemiReadOnlyProperty")), Is.EqualTo("int SemiReadOnlyProperty { get; }"), "public get, private set");
            Assert.That(language.GetSignature(type.GetProperty("WriteOnlyProperty")), Is.EqualTo("int WriteOnlyProperty { set; }"), "no get, public set");
            Assert.That(language.GetSignature(type.GetProperty("SemiWriteOnlyProperty")), Is.EqualTo("int SemiWriteOnlyProperty { set; }"), "private get, public set");
            Assert.That(language.GetSignature(type.GetProperty("InternalProperty", NonPublicInstance)), Is.EqualTo("int InternalProperty { }"), "internal get, private set");
            Assert.That(language.GetSignature(type.GetProperty("ProtectedProperty", NonPublicInstance)), Is.EqualTo("int ProtectedProperty { }"), "protected get, set");
            Assert.That(language.GetSignature(type.GetProperty("SemiProtectedProperty")), Is.EqualTo("int SemiProtectedProperty { get; }"), "public get, protected set");
            Assert.That(language.GetSignature(type.GetProperty("StaticProperty")), Is.EqualTo("static int StaticProperty { get; set; }"), "static, public get, set");
        }

        [Test]
        public void ShouldProvideSignaturesForIndexerMembers()
        {
            var type = typeof(TestData.Xyz.Foo.TestClass);
            Assert.That(language.GetSignature(type.GetProperty("Item")), Is.EqualTo("string this[int index] { get; set; }"));
        }

        [Test]
        public void ShouldProvideSignaturesForEventMembers()
        {
            // event modifiers according to the C# 4.0 reference:
            // new, public, protected, internal, private, static, virtual, sealed, override, abstract, and extern.
            // we ignore all except static and abstract.

            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetEvent("PublicEvent")), Is.EqualTo("event System.EventHandler PublicEvent"));
            Assert.That(language.GetSignature(type.GetEvent("PrivateEvent", NonPublicInstance)), Is.EqualTo("event System.EventHandler PrivateEvent"));
            Assert.That(language.GetSignature(type.GetEvent("StaticEvent", PublicStatic)), Is.EqualTo("static event System.EventHandler StaticEvent"));
            Assert.That(language.GetSignature(type.GetEvent("SealedEvent")), Is.EqualTo("event System.EventHandler SealedEvent"));
            Assert.That(language.GetSignature(type.GetEvent("AbstractEvent")), Is.EqualTo("abstract event System.EventHandler AbstractEvent"));
        }

        [Test]
        public void ShouldProvideSignaturesForOperatorMembers()
        {
            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetMethod("op_LogicalNot")), Is.EqualTo("static MemberSignatureTestClass operator !(MemberSignatureTestClass t)"));
            Assert.That(language.GetSignature(type.GetMethod("op_Addition")), Is.EqualTo("static MemberSignatureTestClass operator +(MemberSignatureTestClass t, int q)"));
            Assert.That(language.GetSignature(type.GetMethod("op_Explicit")), Is.EqualTo("static explicit operator int(MemberSignatureTestClass t)"));
            Assert.That(language.GetSignature(type.GetMethod("op_Implicit")), Is.EqualTo("static implicit operator bool(MemberSignatureTestClass t)"));
        }

        [Test]
        public void ShouldProvideSignaturesForConstructorAndDestructorMembers()
        {
            var type = typeof(TestData.Xyz.Foo.MemberSignatureTestClass);
            Assert.That(language.GetSignature(type.GetConstructor(new Type[] { })), Is.EqualTo("MemberSignatureTestClass()"));
            Assert.That(language.GetSignature(type.GetConstructor(new[] { type })), Is.EqualTo("MemberSignatureTestClass(MemberSignatureTestClass other)"));
            Assert.That(language.GetSignature(type.GetMethod("Finalize", NonPublicInstance)), Is.EqualTo("~MemberSignatureTestClass()"));

            type = typeof(TestData.Xyz.Foo.TestGeneric<,>);
            Assert.That(language.GetSignature(type.GetConstructors().Single()), Is.EqualTo("TestGeneric(T t, G g)"));
            Assert.That(language.GetSignature(type.GetMethod("Finalize", NonPublicInstance)), Is.EqualTo("~TestGeneric()"));

            type = typeof(TestData.Xyz.Foo.TestGeneric<System.Xml.XmlAttribute, TestData.Xyz.Foo.ITest>);
            Assert.That(language.GetSignature(type.GetConstructors().Single()), Is.EqualTo("TestGeneric(System.Xml.XmlAttribute t, ITest g)"));
            Assert.That(language.GetSignature(type.GetMethod("Finalize", NonPublicInstance)), Is.EqualTo("~TestGeneric()"));

            type = typeof(TestData.Xyz.Foo.StaticTestClass);
            Assert.That(language.GetSignature(type.GetConstructors(NonPublicStatic).Single()), Is.EqualTo("static StaticTestClass()"));
        }

        [Test]
        public void ShouldProvideTypeSignaturesForNestedTypes()
        {
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.BirdsNest.First)), Is.EqualTo("class BirdsNest.First"), "public nested class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.BirdsNest.First.Inner)), Is.EqualTo("class BirdsNest.First.Inner"), "nested two levels");
            var type = typeof(TestData.Xyz.Foo.BirdsNest).GetNestedType("Second", NonPublicInstance);
            Assert.That(language.GetSignature(type), Is.EqualTo("class BirdsNest.Second"), "protected nested class");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.BirdsNest.Third)), Is.EqualTo("struct BirdsNest.Third"), "internal nested struct");
            Assert.That(language.GetSignature(typeof(TestData.Xyz.Foo.BirdsNest.NestedDelegate)), Is.EqualTo("delegate int BirdsNest.NestedDelegate(int x)"), "nested delegate type");
        }

        #endregion

        [Test]
        public void ShouldProvideDisplayNames()
        {
            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.TestClass)), Is.EqualTo("TestData.Xyz.Foo.TestClass"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.TestClass)), Is.EqualTo("TestClass"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.TestClass)), Is.EqualTo("class"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.BirdsNest.First.Inner)), Is.EqualTo("TestData.Xyz.Foo.BirdsNest.First.Inner"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.BirdsNest.First.Inner)), Is.EqualTo("BirdsNest.First.Inner"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.BirdsNest.First.Inner)), Is.EqualTo("class"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.ITest)), Is.EqualTo("TestData.Xyz.Foo.ITest"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.ITest)), Is.EqualTo("ITest"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.ITest)), Is.EqualTo("interface"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.TestEnum)), Is.EqualTo("TestData.Xyz.Foo.TestEnum"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.TestEnum)), Is.EqualTo("TestEnum"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.TestEnum)), Is.EqualTo("enum"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.TestStruct)), Is.EqualTo("TestData.Xyz.Foo.TestStruct"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.TestStruct)), Is.EqualTo("TestStruct"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.TestStruct)), Is.EqualTo("struct"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.TestGeneric<,>)), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<T, G>"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.TestGeneric<,>)), Is.EqualTo("TestGeneric<T, G>"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.TestGeneric<,>)), Is.EqualTo("class"));

            Assert.That(language.GetDisplayName(typeof(TestData.Xyz.Foo.TestGeneric<int, int>)), Is.EqualTo("TestData.Xyz.Foo.TestGeneric<int, int>"));
            Assert.That(language.GetShortDisplayName(typeof(TestData.Xyz.Foo.TestGeneric<int, int>)), Is.EqualTo("TestGeneric<int, int>"));
            Assert.That(language.GetMetaTypeName(typeof(TestData.Xyz.Foo.TestGeneric<int, int>)), Is.EqualTo("class"));
        }
    }
}
