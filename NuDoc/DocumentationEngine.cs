﻿namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// Produces various kinds of documentation.
    /// </summary>
    public static class DocumentationEngine
    {
        /// <summary>
        /// Write a filtered slashdoc file with the public API only, for use with IntelliSense.
        /// </summary>
        /// <param name="assemblyReflector"></param>
        /// <param name="slashdocStream"></param>
        /// <param name="fileName"></param>
        public static void WritePublicApiSlashdoc(IAssemblyReflector assemblyReflector, Stream slashdocStream, string fileName)
        {
            var publicSlashdocIds = GetSlashdocIdsForAllVisibleTypesAndMembers(assemblyReflector);
            using (var reader = new SlashdocReader(slashdocStream))
            using (var writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartElement("doc");

                reader.FoundAssemblyName += (x) =>
                    {
                        writer.WriteStartElement("assembly");
                        writer.WriteStartElement("name");
                        writer.WriteString(x);
                        writer.WriteEndElement(); // name
                        writer.WriteEndElement(); // assembly
                    };

                int memberCount = 0;
                reader.FoundMember += (x) =>
                    {
                        if (publicSlashdocIds.Contains(x.Key))
                        {
                            if (memberCount == 0)
                            {
                                writer.WriteStartElement("members");
                            }

                            memberCount++;
                            writer.WriteStartElement("member");
                            writer.WriteAttributeString("name", x.Key);
                            writer.WriteRaw(x.Value);
                            writer.WriteEndElement(); // member
                        }
                    };

                reader.Parse();
            }
        }

        /// <summary>
        /// Write an API reference with simple html formatting.
        /// </summary>
        /// <param name="assemblyReflector"></param>
        /// <param name="slashdocStream"></param>
        /// <param name="fileName"></param>
        public static void WritePublicApiReferenceHtml(IAssemblyReflector assemblyReflector, Stream slashdocStream, string fileName)
        {
            var slashdoc = SlashdocReader.Parse(slashdocStream);
            var language = new CSharpSignatureProvider();
            WritePublicApiReferenceHtml(assemblyReflector, fileName, slashdoc, language);
        }

        /// <summary>
        /// Write an API reference with simple html formatting.
        /// </summary>
        /// <param name="assemblyReflector"></param>
        /// <param name="fileName"></param>
        public static void WritePublicApiReferenceHtml(IAssemblyReflector assemblyReflector, string fileName)
        {
            var slashdoc = new SlashdocDictionary();
            var language = new CSharpSignatureProvider();
            WritePublicApiReferenceHtml(assemblyReflector, fileName, slashdoc, language);
        }

        private static void WritePublicApiReferenceHtml(IAssemblyReflector assemblyReflector, string fileName, SlashdocDictionary slashdoc, ILanguageSignatureProvider language)
        {
            var title = string.Format("{0} public API reference", assemblyReflector.SimpleName);
            using (var apiReferenceWriter = new ApiReferenceHtmlWriter(fileName, title, slashdoc, language))
            {
                apiReferenceWriter.DescribeAssembly(assemblyReflector);
            }
        }

        private static HashSet<string> GetSlashdocIdsForAllVisibleTypesAndMembers(IAssemblyReflector assembly)
        {
            var ids = new HashSet<string>();

            foreach (var type in assembly.Types
                .Where(t => ReflectionHelper.IsVisible(t)))
            {
                ids.Add(SlashdocIdentifierProvider.GetId(type));

                if (type.IsEnum)
                {
                    AddSlashdocIds(ReflectionHelper.GetEnumMembers(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                }
                else
                {
                    AddSlashdocIds(ReflectionHelper.GetVisibleConstructors(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                    AddSlashdocIds(ReflectionHelper.GetVisibleProperties(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                    AddSlashdocIds(ReflectionHelper.GetVisibleMethods(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                    AddSlashdocIds(ReflectionHelper.GetVisibleOperators(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                    AddSlashdocIds(ReflectionHelper.GetVisibleFields(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                    AddSlashdocIds(ReflectionHelper.GetVisibleEvents(type), (x) => SlashdocIdentifierProvider.GetId(x), ids);
                }
            }

            return ids;
        }

        private static void AddSlashdocIds<T>(IEnumerable<T> items, Func<T, string> slashdocIdProvider, HashSet<string> ids)
        {
            foreach (var x in items)
            {
                ids.Add(slashdocIdProvider(x));
            }
        }

    }
}
