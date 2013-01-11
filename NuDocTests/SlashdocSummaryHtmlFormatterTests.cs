namespace NuDocTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using NuDoc;
    using Moq;

    [TestFixture]
    public class SlashdocSummaryHtmlFormatterTests
    {
        private static readonly IAssemblyReflector DummyAssembly = new Mock<IAssemblyReflector>().Object;
        private static readonly ILanguageSignatureProvider DummyLanguage = new Mock<ILanguageSignatureProvider>().Object;

        [Test]
        public void ShouldReturnAnEmptySummaryWhenTheXmlDescriptionIsNullOrEmpty()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary(null), Is.EqualTo(string.Empty));
            Assert.That(formatter.FormatSummary(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ShouldOnlyIncludeContentWithinSummaryTags()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary("<far-out>dude</far-out>"), Is.EqualTo(string.Empty));
            Assert.That(formatter.FormatSummary("<far-out><summary>dude</summary></far-out>"), Is.EqualTo("dude"));
            Assert.That(formatter.FormatSummary("<summary>first</summary><summary><summary>second<summary/></summary></summary>"), Is.EqualTo("firstsecond"));
            Assert.That(formatter.FormatSummary("irrelevant <summary/> irrelevant"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ShouldEscapeXmlDocumentEscapeCharacters()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary("<summary>&lt;hello&gt; &amp; goodbye</summary>"), Is.EqualTo("&lt;hello&gt; &amp; goodbye"));
        }

        [Test]
        public void ShouldMarkupCode()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary("<summary><c>Hello</c>!</summary>"), Is.EqualTo("<code>Hello</code>!"));
            Assert.That(formatter.FormatSummary("<summary><code>Hello</code>!</summary>"), Is.EqualTo("<code>Hello</code>!"));
        }

        [Test]
        public void ShouldMarkupParagraphs()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary("<summary><para>Hello</para>!</summary>"), Is.EqualTo("<p>Hello</p>!"));
        }

        [Test]
        public void ShouldIncludeParameterNamesAsText()
        {
            var formatter = new SlashdocSummaryHtmlFormatter(DummyAssembly, DummyLanguage);
            Assert.That(formatter.FormatSummary("<summary>Hello, <paramref name=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <typeparamref name=\"World\" />!</summary>"), Is.EqualTo("Hello, World!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <paramref />!</summary>"), Is.EqualTo("Hello, !"), "The parser doesn't choke when the attribute is missing.");
        }

        [Test]
        public void ShouldCreateFragmentLinksForTypeReferencesWithinTheSameAssembly()
        {
            var assemblyReflectorMock = new Mock<IAssemblyReflector>();
            assemblyReflectorMock.Setup(x => x.LookupType("System.Guid")).Returns(typeof(System.Guid));
            var formatter = new SlashdocSummaryHtmlFormatter(assemblyReflectorMock.Object, new CSharpSignatureProvider());

            // T:System.Guid: type lookup is successful
            Assert.That(formatter.FormatSummary("<summary>Hello, <see cref=\"T:System.Guid\" />!</summary>"), Is.EqualTo("Hello, <a href=\"#System.Guid\">Guid</a>!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <seealso cref=\"T:System.Guid\" />!</summary>"), Is.EqualTo("Hello, <a href=\"#System.Guid\">Guid</a>!"));

            // T:System.EventArgs: type lookup fails
            Assert.That(formatter.FormatSummary("<summary>Hello, <see cref=\"T:System.EventArgs\" />!</summary>"), Is.EqualTo("Hello, System.EventArgs!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <seealso cref=\"T:System.EventArgs\" />!</summary>"), Is.EqualTo("Hello, System.EventArgs!"));

            // !:Error, E:Foo.Bar: no type lookup, just strip the meta-type identifier
            Assert.That(formatter.FormatSummary("<summary>Hello, <see cref=\"!:Error\" />!</summary>"), Is.EqualTo("Hello, Error!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <seealso cref=\"!:Error\" />!</summary>"), Is.EqualTo("Hello, Error!"));
            Assert.That(formatter.FormatSummary("<summary>Hello, <see cref=\"E:Foo.Bar\" />!</summary>"), Is.EqualTo("Hello, Foo.Bar!"));

            Assert.That(formatter.FormatSummary("<summary>Hello, <see />!</summary>"), Is.EqualTo("Hello, !"), "The parser doesn't choke when the attribute is missing.");
        }
    }
}
