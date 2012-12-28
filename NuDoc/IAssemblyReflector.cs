// -----------------------------------------------------------------------
// <copyright file="IAssemblyReflector.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAssemblyReflector : IDisposable
    {
        /// <summary>
        /// Get all types implemented by the assembly.
        /// </summary>
        IEnumerable<Type> Types { get; }
    }
}
