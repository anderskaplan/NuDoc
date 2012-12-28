using System.IO;
using System;

namespace NuDoc
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO get the assembly file name from the command line instead ... eventually.
            var assemblyFileName = @"..\..\..\SampleAssembly\bin\Debug\SampleAssembly.dll";

            using (var assembly = new AssemblyReflector(assemblyFileName))
            {
                var slashdocFileName = Path.ChangeExtension(assemblyFileName, ".xml");

                if (File.Exists(slashdocFileName))
                {
                    // write a filtered slashdoc file with the public API only, for use with IntelliSense.
                    using (var slashdocStream = new FileStream(slashdocFileName, FileMode.Open, FileAccess.Read))
                    {
                        DocumentationEngine.WritePublicApiSlashdoc(assembly, slashdocStream, @"slashdoc.xml");
                    }

                    // write an API reference with simple HTML formatting.
                    using (var slashdocStream = new FileStream(slashdocFileName, FileMode.Open, FileAccess.Read))
                    {
                        DocumentationEngine.WritePublicApiReferenceHtml(assembly, slashdocStream, @"reference.html");
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("WARNING: Could not open slashdoc file {0}.", slashdocFileName));

                    // write an API reference with simple HTML formatting.
                    DocumentationEngine.WritePublicApiReferenceHtml(assembly, @"reference.html");
                }
            }
        }
    }
}
