namespace NuDoc
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A collection of slashdoc type/member definitions.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This is a dictionary all right.")]
    public class SlashdocDictionary
    {
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        public string AssemblyName { get; set; }

        public void SetXmlDescription(string key, string xmlDescription)
        {
            _dictionary[key] = xmlDescription;
        }

        public string GetXmlDescription(string key)
        {
            string value = null;
            _dictionary.TryGetValue(key, out value); // ignore the return value. we'll return a null string if the key didn't exist.
            return value;
        }
    }
}
