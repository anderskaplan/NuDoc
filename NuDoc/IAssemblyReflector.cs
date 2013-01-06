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
        /// Gets the simple name of the assembly. This is usually, but not necessarily, the file name of the manifest file of the assembly, minus its extension.
        /// </summary>
        string SimpleName { get; }

        /// <summary>
        /// Gets all types implemented by the assembly.
        /// </summary>
        IEnumerable<Type> Types { get; }

        /// <summary>
        /// Gets a type by name, if implemented in the assembly. Otherwise null.
        /// </summary>
        Type LookupType(string name);
    }
}
