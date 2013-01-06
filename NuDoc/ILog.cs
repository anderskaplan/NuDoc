namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Logging interface.
    /// </summary>
    public interface ILog
    {
        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}
