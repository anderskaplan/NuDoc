namespace NuDoc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides information about an assembly.
    /// </summary>
    public interface IAssemblyReflector : IDisposable
    {
        /// <summary>
        /// The simple name of the assembly. This is usually, but not necessarily, the file name of the manifest file of the assembly, minus its extension.
        /// </summary>
        string SimpleName { get; }

        /// <summary>
        /// Get all types implemented by the assembly.
        /// </summary>
        IEnumerable<Type> Types { get; }

        /// <summary>
        /// Get a type by name, if implemented in the assembly. Otherwise null.
        /// </summary>
        Type LookupType(string name);
    }
}
