// -----------------------------------------------------------------------
// <copyright file="SlashdocIdStringFactoryTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NuDocTests
{
    using NuDoc;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class SlashdocIdentifierProviderTests
    {
        private Type type = typeof(N.X);

        [Test]
        public void ShouldProvideIdentifiersForTypes()
        {
            // class, delegate, interface, enum, struct: they're all just types.
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.X)), Is.EqualTo("T:N.X"));
        }

        [Test]
        public void ShouldProvideIdentifiersForNestedTypes()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.X.Nested)), Is.EqualTo("T:N.X.Nested"));
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.X.D)), Is.EqualTo("T:N.X.D"));
        }

        [Test]
        public void ShouldProvideIdentifiersForGenericTypesAndMembers()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.GenericClass<,>)), Is.EqualTo("T:N.GenericClass`2"));
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.GenericClass<,>).GetMethod("Foo")), Is.EqualTo("M:N.GenericClass`2.Foo(`0)"));
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.GenericClass<,>).GetMethod("HalfOpen")), Is.EqualTo("M:N.GenericClass`2.HalfOpen(N.GenericClass{`0,System.Int32})"));
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.GenericClass<,>).GetProperty("Property")), Is.EqualTo("P:N.GenericClass`2.Property"));
            Assert.That(SlashdocIdentifierProvider.GetId(typeof(N.ClassWithGenericMethod).GetMethod("Foo")), Is.EqualTo("M:N.ClassWithGenericMethod.Foo``1(``0)"));
        }

        [Test]
        public void ShouldProvideIdentifiersForArrays()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetMethod("gg")), Is.EqualTo("M:N.X.gg(System.Int16[],System.Int32[0:,0:])"));
        }

        [Test]
        public void ShouldProvideIdentifiersForPointers()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetMethod("bb")), Is.EqualTo("M:N.X.bb(System.String,System.Int32@,System.Void*)"));
        }

        [Test]
        public void ShouldProvideIdentifiersForFieldsAndConstants()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetField("q")), Is.EqualTo("F:N.X.q"));
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetField("PI")), Is.EqualTo("F:N.X.PI"));
        }

        [Test]
        public void ShouldProvideIdentifiersForMethodsAndConstructors()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetConstructor(new Type[] { })), Is.EqualTo("M:N.X.#ctor"));
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetConstructor(new Type[] { typeof(int) })), Is.EqualTo("M:N.X.#ctor(System.Int32)"));
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetMethod("f")), Is.EqualTo("M:N.X.f"));
        }

        [Test]
        public void ShouldProvideIdentifiersForOperators()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetMethod("op_Addition")), Is.EqualTo("M:N.X.op_Addition(N.X,N.X)"));
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetMethod("op_Explicit")), Is.EqualTo("M:N.X.op_Explicit(N.X)~System.Int32"));
        }

        [Test]
        public void ShouldProvideIdentifiersForProperties()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetProperty("prop")), Is.EqualTo("P:N.X.prop"));
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetProperty("Item")), Is.EqualTo("P:N.X.Item(System.String)"));
        }

        [Test]
        public void ShouldProvideIdentifiersForEvents()
        {
            Assert.That(SlashdocIdentifierProvider.GetId(type.GetEvent("d")), Is.EqualTo("E:N.X.d"));
        }

        [Test]
        public void ShouldGetTypeNamesFromSlashdocIds()
        {
            Assert.That(SlashdocIdentifierProvider.GetTypeName("T:N.X"), Is.EqualTo("N.X"));
            Assert.That(SlashdocIdentifierProvider.GetTypeName("T:N.X.Nested"), Is.EqualTo("N.X.Nested"));
            Assert.That(SlashdocIdentifierProvider.GetTypeName("T:N.X.D"), Is.EqualTo("N.X.D"));
            Assert.That(SlashdocIdentifierProvider.GetTypeName("T:N.GenericClass`2"), Is.EqualTo("N.GenericClass`2"));
            Assert.That(SlashdocIdentifierProvider.GetTypeName("T:No.Such.Type"), Is.EqualTo("No.Such.Type"));

            Assert.That(SlashdocIdentifierProvider.GetTypeName(null), Is.Null);
            Assert.That(SlashdocIdentifierProvider.GetTypeName("P:N.X.prop"), Is.Null);
        }
    }
}


namespace N
{
    internal class GenericClass<T, G>
    {
        public G Foo(T t)
        {
            return default(G);
        }

        public void HalfOpen(GenericClass<T, int> foo)
        {
        }

        public G Property
        {
            get { return default(G); }
        }
    }

    internal class ClassWithGenericMethod
    {
        public bool Foo<T>(T t)
        {
            return false;
        }
    }


    //
    // The following is a test class from MSDN (http://msdn.microsoft.com/en-us/library/fsbx0t7x%28v=vs.100%29.aspx)
    //

    /// <summary>
    /// Enter description here for class X. 
    /// ID string generated is "T:N.X". 
    /// </summary>
    internal unsafe class X
    {
        /// <summary>
        /// Enter description here for the first constructor.
        /// ID string generated is "M:N.X.#ctor".
        /// </summary>
        public X() { }


        /// <summary>
        /// Enter description here for the second constructor.
        /// ID string generated is "M:N.X.#ctor(System.Int32)".
        /// </summary>
        /// <param name="i">Describe parameter.</param>
        public X(int i) { }


        /// <summary>
        /// Enter description here for field q.
        /// ID string generated is "F:N.X.q".
        /// </summary>
        public string q;


        /// <summary>
        /// Enter description for constant PI.
        /// ID string generated is "F:N.X.PI".
        /// </summary>
        public const double PI = 3.14;


        /// <summary>
        /// Enter description for method f.
        /// ID string generated is "M:N.X.f".
        /// </summary>
        /// <returns>Describe return value.</returns>
        public int f() { return 1; }


        /// <summary>
        /// Enter description for method bb.
        /// ID string generated is "M:N.X.bb(System.String,System.Int32@,System.Void*)".
        /// </summary>
        /// <param name="s">Describe parameter.</param>
        /// <param name="y">Describe parameter.</param>
        /// <param name="z">Describe parameter.</param>
        /// <returns>Describe return value.</returns>
        public int bb(string s, ref int y, void* z) { return 1; }


        /// <summary>
        /// Enter description for method gg.
        /// ID string generated is "M:N.X.gg(System.Int16[],System.Int32[0:,0:])". 
        /// </summary>
        /// <param name="array1">Describe parameter.</param>
        /// <param name="array">Describe parameter.</param>
        /// <returns>Describe return value.</returns>
        public int gg(short[] array1, int[,] array) { return 0; }


        /// <summary>
        /// Enter description for operator.
        /// ID string generated is "M:N.X.op_Addition(N.X,N.X)". 
        /// </summary>
        /// <param name="x">Describe parameter.</param>
        /// <param name="xx">Describe parameter.</param>
        /// <returns>Describe return value.</returns>
        public static X operator +(X x, X xx) { return x; }


        /// <summary>
        /// Enter description for property.
        /// ID string generated is "P:N.X.prop".
        /// </summary>
        public int prop { get { return 1; } set { } }


        /// <summary>
        /// Enter description for event.
        /// ID string generated is "E:N.X.d".
        /// </summary>
        public event D d;


        /// <summary>
        /// Enter description for property.
        /// ID string generated is "P:N.X.Item(System.String)".
        /// </summary>
        /// <param name="s">Describe parameter.</param>
        /// <returns></returns>
        public int this[string s] { get { return 1; } }


        /// <summary>
        /// Enter description for class Nested.
        /// ID string generated is "T:N.X.Nested".
        /// </summary>
        public class Nested { }


        /// <summary>
        /// Enter description for delegate.
        /// ID string generated is "T:N.X.D". 
        /// </summary>
        /// <param name="i">Describe parameter.</param>
        public delegate void D(int i);


        /// <summary>
        /// Enter description for operator.
        /// ID string generated is "M:N.X.op_Explicit(N.X)~System.Int32".
        /// </summary>
        /// <param name="x">Describe parameter.</param>
        /// <returns>Describe return value.</returns>
        public static explicit operator int(X x) { return 1; }

    }
}
