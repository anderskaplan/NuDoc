using System.IO;
using System;

namespace NuDoc
{
    public class Program
    {
        private ConsoleLogger _logger;

        private Program(ConsoleLogger logger)
        {
            this._logger = logger;
        }

        private string AssemblyFileName { get; set; }

        private string OutputPath { get; set; }

        private bool EnableMissingSummaryWarnings { get; set; }

        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var instance = new Program(logger);
            if (instance.ParseArguments(args))
            {
                instance.Run();
            }
            else
            {
                Console.WriteLine("Usage: NuDoc [/o output-path] [/m] assembly-file");
                Console.WriteLine("Generates API documentation for .NET assemblies.");
                Console.WriteLine();
                Console.WriteLine("The input file is an assembly file, typically with a .dll or .exe extension.");
                Console.WriteLine("NuDoc looks for a matching XML documentation (\"slashdoc\") file in the same");
                Console.WriteLine("directory. Such a file can be generated from special documentation comments");
                Console.WriteLine("embedded in the source code by checking the \"XML documentation file\" option in");
                Console.WriteLine("the project settings in Visual Studio, or by specifying \"/doc\" on the command");
                Console.WriteLine("line when building the assembly. NuDoc uses the summary part of the XML");
                Console.WriteLine("documentation comments only.");
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
                Console.WriteLine("  /m  Enable warnings for missing and empty XML documentation summaries.");
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
                                _logger.Error("The option /o must be followed by a path.");
                                return false;
                            }
                            break;

                        case "M":
                            EnableMissingSummaryWarnings = true;
                            break;

                        default:
                            _logger.Error(string.Format("Unknown option \"{0}\".", args[i]));
                            return false;
                    }
                }
                else
                {
                    if (AssemblyFileName == null)
                    {
                        AssemblyFileName = args[i];
                    }
                    else
                    {
                        _logger.Error("Multiple input files specified.");
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

                using (var assembly = new AssemblyReflector(AssemblyFileName, _logger))
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
                            SlashdocProcessor.WritePublicApiSlashdoc(assembly, slashdocStream, publicApiSlashdocFileName);
                        }

                        using (var slashdocStream = new FileStream(slashdocFileName, FileMode.Open, FileAccess.Read))
                        {
                            slashdoc = SlashdocReader.Parse(slashdocStream);
                        }
                    }
                    else
                    {
                        _logger.Warning(string.Format("Could not open slashdoc file '{0}'.", slashdocFileName));
                    }

                    var language = new CSharpSignatureProvider();
                    var title = string.Format("{0} public API reference", assembly.SimpleName);
                    using (var apiReferenceWriter = new ApiReferenceHtmlWriter(publicApiReferenceFileName, title, slashdoc, language, _logger))
                    {
                        apiReferenceWriter.EnableMissingSummaryWarnings = EnableMissingSummaryWarnings;
                        apiReferenceWriter.DescribeAssembly(assembly);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
    }
}
