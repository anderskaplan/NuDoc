namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Simple logger writing to stdout/stderr.
    /// </summary>
    public class ConsoleLogger : ILog
    {
        public void LogInfo(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.Error.WriteLine("WARNING: " + message);
        }

        public void LogError(string message)
        {
            Console.Error.WriteLine("ERROR: " + message);
        }
    }
}
