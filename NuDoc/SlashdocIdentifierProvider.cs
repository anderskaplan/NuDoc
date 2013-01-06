namespace NuDoc
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides slashdoc ID strings for types and members.
    /// </summary>
    public static class SlashdocIdentifierProvider
    {
        public static string GetId(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture, "T:{0}", GetTypeName(type));
        }

        public static string GetId(MethodBase methodOrConstructor)
        {
            var genericPart = string.Empty;
            if (methodOrConstructor.IsGenericMethod)
            {
                genericPart = string.Format(CultureInfo.InvariantCulture, "``{0}", methodOrConstructor.GetGenericArguments().Length);
            }

            var parametersPart = string.Join(
                ",", 
                methodOrConstructor.GetParameters()
                    .Select(x => GetTypeName(x.ParameterType)));
            if (!string.IsNullOrEmpty(parametersPart))
            {
                parametersPart = "(" + parametersPart + ")";
            }

            var conversionReturnTypePart = string.Empty;
            if (methodOrConstructor.IsSpecialName && 
                (methodOrConstructor.Name.Equals("op_Explicit") || methodOrConstructor.Name.Equals("op_Implicit")))
            {
                conversionReturnTypePart = "~" + GetTypeName(((MethodInfo)methodOrConstructor).ReturnType);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "M:{0}.{1}{2}{3}{4}",
                GetTypeName(methodOrConstructor.ReflectedType),
                HashEncode(methodOrConstructor.Name),
                genericPart,
                parametersPart,
                conversionReturnTypePart);
        }

        public static string GetId(PropertyInfo property)
        {
            var parameters = string.Join(",", property.GetIndexParameters().Select(x => GetTypeName(x.ParameterType)));

            if (!string.IsNullOrEmpty(parameters))
            {
                parameters = "(" + parameters + ")";
            }

            return string.Format(CultureInfo.InvariantCulture, "P:{0}.{1}{2}", GetTypeName(property.ReflectedType), HashEncode(property.Name), parameters);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specialized type needed to differentiate between object types.")]
        public static string GetId(FieldInfo field)
        {
            return string.Format(CultureInfo.InvariantCulture, "F:{0}.{1}", GetTypeName(field.ReflectedType), HashEncode(field.Name));
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Specialized type needed to differentiate between object types.")]
        public static string GetId(EventInfo event1)
        {
            return string.Format(CultureInfo.InvariantCulture, "E:{0}.{1}", GetTypeName(event1.ReflectedType), HashEncode(event1.Name));
        }

        public static string GetTypeName(string slashdocId)
        {
            if (slashdocId == null || !slashdocId.StartsWith("T:", StringComparison.Ordinal))
            {
                return null;
            }

            return slashdocId.Substring(2);
        }

        /// <summary>
        /// Dots in method names shall be replaced with hash signs. (Typically only used by constructors which are called ".ctor".)
        /// </summary>
        private static string HashEncode(string name)
        {
            return name.Replace('.', '#');
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericParameter)
            {
                var prefix = (type.DeclaringMethod != null) ? "``" : "`";
                return string.Format(CultureInfo.InvariantCulture, "{0}{1}", prefix, type.GenericParameterPosition);
            }

            if (type.IsGenericType && 
                !type.IsGenericTypeDefinition)
            {
                // this is an *instantiation* of a generic type.
                var genericTypeName = GetTypeName(type.GetGenericTypeDefinition());
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                var genericArguments = string.Join(",", type.GetGenericArguments().Select(x => GetTypeName(x)));
                return string.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", genericTypeName, genericArguments);
            }

            if (type.IsArray)
            {
                return GetTypeName(type.GetElementType()) + "[" + GetArrayRankDescription(type) + "]";
            }

            if (type.IsByRef)
            {
                return GetTypeName(type.GetElementType()) + "@";
            }

            var name = type.FullName;

            if (type.IsNested)
            {
                // Replace the '+' with '.'
                name = name.Replace('+', '.');
            }

            return name;
        }

        private static string GetArrayRankDescription(Type type)
        {
            var rankDescription = string.Empty;

            var rank = type.GetArrayRank();
            if (rank > 1)
            {
                rankDescription = string.Join(
                    ",",
                    Enumerable.Range(0, rank)
                        .Select(x => "0:"));
            }
            
            return rankDescription;
        }
    }
}
