using System.IO;
using NuDoc;
using NUnit.Framework;
using System;

namespace NuDocTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //new DocumentationEngineTests().ShouldGenerateSlashdocForPublicTypesAndMembersOnly();
            //new CSharpSignatureProviderTests().ShouldProvideSignaturesForGenericMethodMembers();
            //new CSharpTypeReferenceProviderTests().ShouldReferenceGenericTypes();
            //new ApiReferenceHtmlWriterTests().ShouldXmlEscapeSlashdocSummaries();
            //new SlashdocReaderTests().ShouldFailWhenReadingAnInvalidSlashdocFile();
            //new SlashdocIdentifierProviderTests().ShouldProvideIdentifiersForGenericTypesAndMembers();
            //new AssemblyReflectorTests().ShouldLookTypesByName();
            //new SlashdocSummaryHtmlFormatterTests().ShouldCreateFragmentLinksForTypeReferencesWithinTheSameAssembly();

            Console.WriteLine("---diff---");
            var nudoc = SlashdocReader.Parse(new FileStream(@"..\..\..\NuDoc\bin\Debug\slashdoc.xml", FileMode.Open, FileAccess.Read));
            var other = SlashdocReader.Parse(new FileStream(@"\temp\Tobii.TecSDK.Client-public.xml", FileMode.Open, FileAccess.Read));
            Diff("NuDoc but not other: ", nudoc, other);
            Diff("Other but not NuDoc: ", other, nudoc);
        }

        private static void Diff(string caption, SlashdocDictionary left, SlashdocDictionary right)
        {
            foreach (var key in left.Keys)
            {
                if (!right.ContainsKey(key))
                {
                    Console.WriteLine(string.Format("{0}: {1}", caption, key));
                }
            }
        }
    }
}
