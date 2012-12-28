// -----------------------------------------------------------------------
// <copyright file="Reflector.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AssemblyReflector : IAssemblyReflector
    {
        private Assembly _assembly;

        public AssemblyReflector(string fileName)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            _assembly = Assembly.ReflectionOnlyLoadFrom(fileName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Type> Types
        {
            get
            {
                return _assembly.GetTypes();
            }
        }

        public string SimpleName
        {
            get
            {
                return _assembly.GetName().Name;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= CurrentDomain_ReflectionOnlyAssemblyResolve;
            }
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine(string.Format("Loading assembly for reflection: {0}.", args.Name));
            return Assembly.ReflectionOnlyLoad(args.Name);
        }
    }
}
