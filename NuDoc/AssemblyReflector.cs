namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class AssemblyReflector : IAssemblyReflector
    {
        private static readonly string[] AssemblyExtensions = new[] { ".dll", ".exe" };

        private Assembly _assembly;
        private ILog _logger;
        private List<string> _loadingAttempted = new List<string>();

        public AssemblyReflector(string fileName, ILog logger)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            _assembly = Assembly.ReflectionOnlyLoadFrom(fileName);
            _logger = logger;
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
                catch (ReflectionTypeLoadException)
                {
                    // NOTE: errors will be logged by the CurrentDomain.ReflectionOnlyAssemblyResolve event handler.
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
            catch (ReflectionTypeLoadException)
            {
                // NOTE: errors will be logged by the CurrentDomain.ReflectionOnlyAssemblyResolve event handler.
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

            _logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Loading assembly for reflection: {0}.", args.Name));
            _loadingAttempted.Add(args.Name);
            try
            {
                return LoadAssembly(args.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
