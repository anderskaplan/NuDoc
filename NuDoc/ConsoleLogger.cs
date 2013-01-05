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
        public void Info(string message)
        {
            Console.Out.WriteLine(message);
        }

        public void Warning(string message)
        {
            Console.Error.WriteLine("WARNING: " + message);
        }

        public void Error(string message)
        {
            Console.Error.WriteLine("ERROR: " + message);
        }
    }
}
