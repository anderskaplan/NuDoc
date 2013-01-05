using System.IO;
using System;

namespace NuDoc
{
    class Program
    {
        public string AssemblyFileName { get; set; }

        public string OutputPath { get; set; }

        static void Main(string[] args)
        {
            var instance = new Program();
            if (instance.ParseArguments(args))
            {
                instance.Run();
            }
            else
            {
                Console.WriteLine("Usage: NuDoc [/o output-path] assembly-file");
                Console.WriteLine("Generates API documentation for .NET assemblies.");
                Console.WriteLine();
                Console.WriteLine("The input file is an assembly file, typically with a .dll or .exe extension.");
                Console.WriteLine("NuDoc looks for a matching XML documentation (\"slashdoc\") file in the same");
                Console.WriteLine("directory. Such a file can be generated from special documentation comments");
                Console.WriteLine("embedded in the source code by checking the \"XML documentation file\" option in");
                Console.WriteLine("the project settings in Visual Studio, or by specifying \"/doc\" on the command");
                Console.WriteLine("line when building the assembly.");
                Console.WriteLine();
                Console.WriteLine("NuDoc produces two output files:");
                Console.WriteLine(" - A HTML file describing the public API of the assembly. (I.e., the publicly");
                Console.WriteLine("   visible types and members.)");
                Console.WriteLine(" - An XML documentation file for the assembly which includes only the");
                Console.WriteLine("   information for the public API.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  /o  Specify the path where the output files are to be written. The directory");
                Console.WriteLine("      will be created if it doesn't already exist. The default output path is");
                Console.WriteLine("      the current directory.");
            }
        }

        private bool ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                {
                    switch (args[i].Substring(1).ToUpperInvariant())
                    {
                        case "O":
                            if (i + 1 < args.Length)
                            {
                                OutputPath = args[i + 1];
                                i++;
                            }
                            else
                            {
                                Console.WriteLine("ERROR: The option /o must be followed by a path.");
                                return false;
                            }
                            break;

                        default:
                            Console.WriteLine("ERROR: Unknown option \"{0}\".", args[i]);
                            return false;
                    }
                }
                else
                {
                    if (i == args.Length - 1)
                    {
                        AssemblyFileName = args[i];
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Unknown argument \"{0}\".", args[i]);
                    }
                }
            }

            return AssemblyFileName != null;
        }

        private void Run()
        {
            try
            {
                if (OutputPath != null)
                {
                    if (!Directory.Exists(OutputPath))
                    {
                        Directory.CreateDirectory(OutputPath);
                    }
                }

                using (var assembly = new AssemblyReflector(AssemblyFileName))
                {
                    var publicApiReferenceFileName = Path.GetFileNameWithoutExtension(AssemblyFileName) + ".html";
                    var publicApiSlashdocFileName = Path.GetFileNameWithoutExtension(AssemblyFileName) + ".xml";
                    if (OutputPath != null)
                    {
                        publicApiReferenceFileName = Path.Combine(OutputPath, publicApiReferenceFileName);
                        publicApiSlashdocFileName = Path.Combine(OutputPath, publicApiSlashdocFileName);
                    }

                    var slashdocFileName = Path.ChangeExtension(AssemblyFileName, ".xml");
                    var slashdoc = new SlashdocDictionary();
                    if (File.Exists(slashdocFileName))
                    {
                        using (var slashdocStream = new FileStream(slashdocFileName, FileMode.Open, FileAccess.Read))
                        {
                            DocumentationEngine.WritePublicApiSlashdoc(assembly, slashdocStream, publicApiSlashdocFileName);
                        }

                        using (var slashdocStream = new FileStream(slashdocFileName, FileMode.Open, FileAccess.Read))
                        {
                            slashdoc = SlashdocReader.Parse(slashdocStream);
                        }
                    }
                    else
                    {
                        Console.WriteLine(string.Format("WARNING: Could not open slashdoc file '{0}'.", slashdocFileName));
                    }

                    var language = new CSharpSignatureProvider();
                    DocumentationEngine.WritePublicApiReferenceHtml(assembly, publicApiReferenceFileName, slashdoc, language);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("ERROR: {0}", ex.Message));
            }
        }
    }
}
