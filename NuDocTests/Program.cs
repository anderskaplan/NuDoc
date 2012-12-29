using System.IO;

namespace NuDocTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //new DocumentationEngineTests().ShouldGenerateSlashdocForPublicTypesAndMembersOnly();
            new CSharpSignatureProviderTests().ShouldProvideDisplayNames();
            //new CSharpTypeReferenceProviderTests().ShouldReferenceGenericTypes();
            //new ApiReferenceHtmlWriterTests().ShouldDescribeASetOfTypes();
            //new SlashdocReaderTests().ShouldFailWhenReadingAnInvalidSlashdocFile();
            //new SlashdocIdentifierProviderTests().ShouldProvideIdentifiersForOperators();
        }
    }
}
