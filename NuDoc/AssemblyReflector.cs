namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.IO;

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

        public string SimpleName
        {
            get
            {
                return _assembly.GetName().Name;
            }
        }

        public IEnumerable<Type> Types
        {
            get
            {
                return _assembly.GetTypes();
            }
        }

        public Type LookupType(string name)
        {
            return _assembly.GetType(name, false);
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
