namespace SampleAssembly
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is generic indeed. Perhaps not so useful, but generic.
    /// </summary>
    public class GenericClass<T, G>
        where G : new()
        where T : struct, IDisposable
    {
        /// <summary>
        /// Method with a generic parameter.
        /// </summary>
        public G Foo(T t)
        {
            return default(G);
        }

        /// <summary>
        /// A half-open, half-closed generic method.
        /// </summary>
        public void HalfOpen(GenericClass<T, int> foo)
        {
        }

        /// <summary>
        /// Method with a generic parameter passed by reference.
        /// </summary>
        bool TryGetValue(T t, ref G g)
        {
            return false;
        }

        /// <summary>
        /// Method with an generic parameter declared as "out".
        /// </summary>
        void OutParameter(out G g)
        {
            g = default(G);
        }
    }

    public class ClassWithGenericMethod
    {
        /// <summary>
        /// A generic method.
        /// </summary>
        public bool Foo<T>(T t)
        {
            return false;
        }
    }
}
