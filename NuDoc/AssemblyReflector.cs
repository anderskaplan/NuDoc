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
        private static readonly string[] AssemblyExtensions = new[] { ".dll", ".exe" };

        private Assembly _assembly;
        private List<string> _loadingAttempted = new List<string>();

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
                if (_assembly != null)
                {
                    return _assembly.GetName().Name;
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<Type> Types
        {
            get
            {
                try
                {
                    return _assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return new Type[] { };
                }
            }
        }

        public Type LookupType(string name)
        {
            try
            {
                return _assembly.GetType(name, false);
            }
            catch (ReflectionTypeLoadException ex)
            {
                return null;
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
            if (_loadingAttempted.Contains(args.Name))
            {
                return null;
            }

            Console.WriteLine(string.Format("Loading assembly for reflection: {0}.", args.Name));
            _loadingAttempted.Add(args.Name);
            try
            {
                return LoadAssembly(args.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return null;
            }
        }

        private Assembly LoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.ReflectionOnlyLoad(assemblyName);
            }
            catch (FileNotFoundException)
            {
                var fileName = LookupAssemblyInTheLocalCache(assemblyName);
                if (fileName != null)
                {
                    return Assembly.ReflectionOnlyLoadFrom(fileName);
                }

                throw;
            }
        }

        private string LookupAssemblyInTheLocalCache(string assemblyName)
        {
            if (_assembly == null)
            {
                return null;
            }

            var directory = Path.GetDirectoryName(_assembly.Location);
            return AssemblyExtensions
                .Select(ext =>
                {
                    var simpleName = new AssemblyName(assemblyName).Name;
                    return Path.Combine(directory, simpleName + ext);
                })
                .FirstOrDefault(x => File.Exists(x));
        }
    }
}
