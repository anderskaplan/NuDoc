using System.IO;

namespace NuDocTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //new DocumentationEngineTests().ShouldGenerateSlashdocForPublicTypesAndMembersOnly();
            //new CSharpTests().ShouldReferenceArrayTypes();
            //new CSharpTypeReferenceProviderTests().ShouldReferenceGenericTypes();
            //new ApiReferenceHtmlWriterTests().ShouldDescribeASetOfTypes();
            //new SlashdocReaderTests().ShouldFailWhenReadingAnInvalidSlashdocFile();
            new SlashdocIdentifierProviderTests().ShouldProvideIdentifiersForOperators();
        }
    }
}
