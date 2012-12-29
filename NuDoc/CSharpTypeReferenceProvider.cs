namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class CSharpTypeReferenceProvider
    {
        private static Dictionary<Type, string> _builtInTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(object), "object" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(void), "void" },
        };

        private IList<object> _context;

        /// <summary>
        /// Create a new instance without a context.
        /// </summary>
        public CSharpTypeReferenceProvider()
            : this(null)
        {
        }

        /// <summary>
        /// Create a new instance with the given type as context. Type 
        /// references will be relative to the context type. For example, types 
        /// within the same or a parent namespace will be referenced without 
        /// the namespace.
        /// </summary>
        public CSharpTypeReferenceProvider(Type context)
        {
            _context = GetFullTypeSequence(context);
        }

        /// <summary>
        /// Get a reference to the specified type, relative to the context if 
        /// there is one.
        /// </summary>
        public string GetTypeReference(Type type)
        {
            return GetTypeReference(type, false);
        }

        /// <summary>
        /// Get a reference to the specified type, relative to the context if 
        /// there is one.
        /// </summary>
        public string GetTypeReference(Type type, bool skipNamespace)
        {
            string builtInTypeName = null;
            if (_builtInTypes.TryGetValue(type, out builtInTypeName))
            {
                return builtInTypeName;
            }
            else if (type.IsArray)
            {
                return GetArrayTypeReference(type);
            }
            else if (type.IsPointer)
            {
                return GetTypeReference(type.GetElementType()) + "*";
            }
            else if (type.IsGenericType &&
                !type.IsGenericTypeDefinition &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetTypeReference(type.GetGenericArguments().First()) + "?";
            }

            var sequence = GetFullTypeSequence(type);
            int skipCount = NumberOfItemsSharedWithContext(sequence);

            if (skipNamespace)
            {
                while (skipCount < sequence.Count &&
                    sequence[skipCount] is string)
                {
                    skipCount++;
                }
            }

            return string.Join(
                ".",
                sequence
                    .Skip(skipCount)
                    .Select(item => SequenceItemToString(item)));
        }

        /// <summary>
        /// Type name without namespace, parent types, or generic parameters. Used for constructor/finalizer names and a few other special cases.
        /// </summary>
        public static string GetBaseName(Type type)
        {
            if (type.IsGenericType)
            {
                var name = type.Name;
                var adjustedLength = name.Length - 1;
                while (adjustedLength > 0 && name[adjustedLength] != '`')
                {
                    adjustedLength--;
                }
                var adjustedName = name.Substring(0, adjustedLength);
                return adjustedName;
            }
            else
            {
                return type.Name;
            }
        }

        private string GetArrayTypeReference(Type type)
        {
            var commas = string.Empty;
            var rank = type.GetArrayRank();
            if (rank > 1)
            {
                commas = string.Join(
                    " ",
                    Enumerable.Range(0, rank - 1)
                        .Select(x => ","));
            }
            return GetTypeReference(type.GetElementType()) + "[" + commas + "]";
        }

        private IList<object> GetFullTypeSequence(Type type)
        {
            if (type == null)
            {
                return new object[] { };
            }

            return type.Namespace.Split('.')
                .Concat(GetTypeAndDeclaringTypesSequence(type))
                .ToList();
        }

        private IList<object> GetTypeAndDeclaringTypesSequence(Type type)
        {
            var sequence = new List<object>();
            sequence.Add(type);
            for (var parentType = type.DeclaringType; parentType != null; parentType = parentType.DeclaringType)
            {
                sequence.Add(parentType);
            }
            sequence.Reverse();
            return sequence;
        }

        private int NumberOfItemsSharedWithContext(IList<object> sequence)
        {
            int count = 0;
            while (count < _context.Count &&
                count + 1 < sequence.Count &&
                _context[count].Equals(sequence[count]))
            {
                count++;
            }

            return count;
        }

        private string SequenceItemToString(object item)
        {
            var stringItem = item as string;
            if (stringItem != null)
            {
                return stringItem;
            }

            var type = (Type)item;
            if (type.IsGenericType)
            {
                var name = GetBaseName(type);
                if (type.IsGenericTypeDefinition)
                {
                    var parameters = string.Join(", ", type.GetGenericArguments().Select(x => x.Name));
                    return string.Format(CultureInfo.InvariantCulture, "{0}<{1}>", name, parameters);
                }
                else
                {
                    var parameters = string.Join(", ", type.GetGenericArguments().Select(x => GetTypeReference(x)));
                    return string.Format(CultureInfo.InvariantCulture, "{0}<{1}>", name, parameters);
                }
            }
            else
            {
                return type.Name;
            }
        }
    }
}
