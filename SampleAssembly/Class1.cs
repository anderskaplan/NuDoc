using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleAssembly
{
    /// <summary>
    /// A class.
    /// </summary>
    public class Class1
    {
        /// <summary>
        /// An important property. <see cref="SampleAssembly"/> Yes box allright.
        /// </summary>
        public bool Foo { get; set; }

        /// <summary>
        /// If I could only remember what.
        /// </summary>
        /// <param name="when">And when.</param>
        public void DoSomething(DateTime when)
        {
        }

        /// <summary>
        /// Test for equality.
        /// </summary>
        public static bool operator == (Class1 first, Class1 second)
        {
            return true;
        }

        /// <summary>
        /// Test for inequality.
        /// </summary>
        public static bool operator !=(Class1 first, Class1 second)
        {
            return true;
        }

        /// <summary>
        /// Delegate type.
        /// </summary>
        public delegate int Frotz(int x);

        /// <summary>
        /// A private method.
        /// </summary>
        private void PrivateMethod()
        {
        }
    }
}
