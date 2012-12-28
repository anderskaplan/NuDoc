﻿namespace TestData.Xyz.Foo
{
    using System;

    internal class TestClass : ICloneable
    {
        public TestClass(string xyz)
        {
        }

        ~TestClass()
        {
        }

        public int ReadWriteProperty { get; set; }
        public int ReadOnlyProperty
        {
            get { return 0; }
        }
        public int SemiReadOnlyProperty { get; private set; }
        public int WriteOnlyProperty
        {
            set { }
        }
        public int SemiWriteOnlyProperty { private get; set; }
        public static int StaticProperty { get; set; }
        internal int InternalProperty { get; private set; }
        protected int ProtectedProperty { get; set; }
        public int SemiProtectedProperty { get; protected set; }

        public string this[int index]
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void MethodReturningVoid()
        {
        }

        private void Hidden()
        {
        }

        public int x;
        public const bool y = false;

        public static event EventHandler AnEvent;

        public delegate int Frotz(int x);

        public static TestClass operator !(TestClass t)
        {
            return t;
        }

        public class NestedClass
        {
            public void Foo()
            {
            }
        }
    }

    public class PublicTestClass
    {
    }

    public class SpecializedTestClass : PublicTestClass
    {
    }

    internal class InternalTestClass
    {
    }

    internal static class StaticTestClass
    {
        static StaticTestClass()
        {
        }

        public static void ExtensionMethod(this InternalTestClass subject)
        {
        }
    }

    internal sealed class SealedTestClass
    {
    }

    internal abstract class AbstractTestClass
    {
    }

    public class MemberSignatureTestClassBase
    {
        public virtual void SealedMethod()
        {
        }

        public virtual event EventHandler SealedEvent;
    }

    public abstract class MemberSignatureTestClass : MemberSignatureTestClassBase
    {
        public MemberSignatureTestClass()
        {
        }

        public MemberSignatureTestClass(MemberSignatureTestClass other)
        {
        }

        ~MemberSignatureTestClass()
        {
        }

        public void PublicMethod()
        {
        }

        protected void ProtectedMethod()
        {
        }

        internal void InternalMethod()
        {
        }

        private void PrivateMethod()
        {
        }

        public static void StaticMethod()
        {
        }

        public virtual void VirtualMethod()
        {
        }

        public override sealed void SealedMethod()
        {
        }

        public abstract void AbstractMethod();

        public void MethodWithNullableParameter(TestStruct? s)
        {
        }

        public bool? MethodWithNullableReturnValue()
        {
            return null;
        }

        public int publicField;
        protected int protectedField;
        internal int internalField;
        private int privateField;
        public static int staticField;
        public readonly int readonlyField;
        public static readonly int staticReadonlyField;
        public const int constField = 17;
        internal const int internalConstField = 18;

        public event EventHandler PublicEvent;
        private event EventHandler PrivateEvent
        {
            add { }
            remove { }
        }
        public static event EventHandler StaticEvent;
        public override sealed event EventHandler SealedEvent;
        public abstract event EventHandler AbstractEvent;

        public static MemberSignatureTestClass operator !(MemberSignatureTestClass t)
        {
            return t;
        }

        public static MemberSignatureTestClass operator +(MemberSignatureTestClass t, int q)
        {
            return t;
        }

        public static explicit operator int(MemberSignatureTestClass t)
        {
            return 1;
        }

        public static implicit operator bool(MemberSignatureTestClass t)
        {
            return true;
        }
    }

    public delegate int Delegate1(int x);

    public delegate Y GenericDelegate<T, Y>(T x);

    public interface ITest
    {
        void Foo(int count);
        int Whatever { get; }
        event EventHandler Bang;
    }

    public interface IGeneric<T>
    {
    }

    public interface ITest2 : IDisposable
    {
        void Foo(int count);
    }

    public struct TestStruct : IFormattable
    {
        public event EventHandler PublicEvent;

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }

    public struct TestStructGeneric<T>
    {
    }

    public enum TestEnum
    {
        One = 1,
        Two,
        Three
    }

    public class TestGeneric<T, G>
    {
        private T _t;
        private G _g;

        public TestGeneric(T t, G g)
        {
            _t = t;
            _g = g;
        }

        ~TestGeneric()
        {
        }

        public G Foo(T t)
        {
            return default(G);
        }

        public TestGeneric<int, G> HalfOpen()
        {
            return null;
        }

        public void HalfOpenParameter(TestGeneric<int, G> parameter)
        {
        }
    }

    public class TestClassWithGenericMethod
    {
        public void Bar<Q>(Q q)
        {
        }
    }

    public class BirdsNest
    {
        public class First
        {
            public class Inner
            {
            }
        }

        protected class Second
        {
        }

        internal struct Third
        {
        }

        public delegate int NestedDelegate(int x);
    }
}
