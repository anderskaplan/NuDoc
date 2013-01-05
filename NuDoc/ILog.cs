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
        void Info(string message);

        void Warning(string message);

        void Error(string message);
    }
}
