namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provider of type/member signatures for C#.
    /// </summary>
    public class CSharpSignatureProvider : ILanguageSignatureProvider
    {
        private static Dictionary<string, string> _operators = new Dictionary<string, string>
        {
            // unary operators
            { "op_UnaryPlus", "operator +" },
            { "op_UnaryNegation", "operator -" },
            { "op_LogicalNot", "operator !" },
            { "op_OnesComplement", "operator ~" }, 
            { "op_Increment", "operator ++" },
            { "op_Decrement", "operator --" },
            { "op_True", "operator true" },
            { "op_False", "operator false" },

            // binary operators
            { "op_Addition", "operator +" },
            { "op_Subtraction", "operator -" },
            { "op_Multiply", "operator *" },
            { "op_Division", "operator /" },
            { "op_Modulus", "operator %" },
            { "op_BitwiseAnd", "operator &" },
            { "op_BitwiseOr", "operator |" },
            { "op_ExclusiveOr", "operator ^" },
            { "op_LeftShift", "operator <<" },
            { "op_RightShift", "operator >>" },
            { "op_Equality", "operator ==" },
            { "op_Inequality", "operator !=" },
            { "op_LessThan", "operator <" },
            { "op_LessThanOrEqual", "operator <=" },
            { "op_GreaterThan", "operator >" },
            { "op_GreaterThanOrEqual", "operator >=" },

            // conversion operators
            { "op_Explicit", "explicit operator" },
            { "op_Implicit", "implicit operator" },
        };

        private static readonly string ExtensionAttributeSignature = string.Format(CultureInfo.InvariantCulture, "[{0}()]", typeof(ExtensionAttribute).FullName);

        public string GetDisplayName(Type type)
        {
            return new CSharpTypeReferenceProvider().GetTypeReference(type, false);
        }

        public string GetShortDisplayName(Type type)
        {
            return new CSharpTypeReferenceProvider().GetTypeReference(type, true);
        }

        /// <summary>
        /// The meta-type of a type can be for example "class", or "enum".
        /// </summary>
        public string GetMetaTypeName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsClass)
            {
                if (IsDelegateType(type))
                {
                    return "delegate";
                }
                else
                {
                    return "class";
                }
            }
            else if (type.IsInterface)
            {
                return "interface";
            }
            else // not a class => value type
            {
                if (type.IsEnum)
                {
                    return "enum";
                }
                else // not an enum => struct
                {
                    return "struct";
                }
            }
        }

        public string GetSignature(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            var typeReferencer = new CSharpTypeReferenceProvider(type);
            var sb = new StringBuilder();

            if (type.IsClass)
            {
                if (IsDelegateType(type))
                {
                    AppendMetaType(type, sb);

                    var method = type.GetMethod("Invoke");
                    sb.Append(typeReferencer.GetTypeReference(method.ReturnType));
                    sb.Append(" ");

                    AppendTypeName(type, sb);
                    AppendMethodParameters(method, sb, typeReferencer);
                }
                else
                {
                    AppendNonAccessModifiers(type, sb);

                    AppendMetaType(type, sb);
                    AppendTypeName(type, sb);
                    AppendBaseTypeAndInterfaces(type, sb, typeReferencer);
                }
            }
            else if (type.IsInterface)
            {
                AppendMetaType(type, sb);
                AppendTypeName(type, sb);
                AppendBaseTypeAndInterfaces(type, sb, typeReferencer);
            }
            else // not a class => value type
            {
                if (type.IsEnum)
                {
                    AppendMetaType(type, sb);
                    AppendTypeName(type, sb);
                }
                else // not an enum => struct
                {
                    AppendMetaType(type, sb);
                    AppendTypeName(type, sb);
                    AppendBaseTypeAndInterfaces(type, sb, typeReferencer);
                }
            }

            return sb.ToString();
        }

        private void AppendMetaType(Type type, StringBuilder sb)
        {
            sb.Append(GetMetaTypeName(type)); // class, enum, or whatever.
            sb.Append(' ');
        }

        private void AppendTypeName(Type type, StringBuilder sb)
        {
            sb.Append(GetShortDisplayName(type));
        }

        public string GetSignature(ConstructorInfo constructor)
        {
            if (constructor == null) throw new ArgumentNullException("constructor");

            var typeReferencer = new CSharpTypeReferenceProvider(constructor.ReflectedType);
            var sb = new StringBuilder();

            AppendNonAccessModifiers(constructor, sb);
            sb.Append(CSharpTypeReferenceProvider.GetBaseName(constructor.ReflectedType));
            AppendMethodParameters(constructor, sb, typeReferencer);

            return sb.ToString();
        }

        public string GetSignature(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException("property");

            var typeReferencer = new CSharpTypeReferenceProvider(property.ReflectedType);
            var sb = new StringBuilder();

            var getter = property.GetGetMethod(false);
            var setter = property.GetSetMethod(false);

            if (!property.ReflectedType.IsInterface)
            {
                var accessor = getter ?? setter;
                if (accessor != null)
                {
                    AppendNonAccessModifiers(accessor, sb);
                }
            }

            sb.Append(typeReferencer.GetTypeReference(property.PropertyType));
            sb.Append(" ");

            var indexers = property.GetIndexParameters();
            if (indexers.Length > 0)
            {
                sb.Append("this[");
                sb.Append(FormatParameters(indexers, typeReferencer));
                sb.Append("]");
            }
            else
            {
                sb.Append(property.Name);
            }

            sb.Append(" { ");
            if (getter != null)
            {
                sb.Append("get; ");
            }
            if (setter != null)
            {
                sb.Append("set; ");
            }
            sb.Append("}");

            return sb.ToString();
        }

        public string GetSignature(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException("method");

            var typeReferencer = new CSharpTypeReferenceProvider(method.ReflectedType);
            var sb = new StringBuilder();

            if (method.Name == "Finalize")
            {
                sb.Append("~");
                sb.Append(CSharpTypeReferenceProvider.GetBaseName(method.ReflectedType));
                sb.Append("()");
                return sb.ToString();
            }

            if (!method.ReflectedType.IsInterface)
            {
                AppendNonAccessModifiers(method, sb);
            }

            var operatorNameBeforeType = false;
            if (method.IsSpecialName && 
                (method.Name.Equals("op_Explicit") || method.Name.Equals("op_Implicit")))
            {
                sb.Append(_operators[method.Name]);
                sb.Append(" ");
                operatorNameBeforeType = true;
            }

            sb.Append(typeReferencer.GetTypeReference(method.ReturnType));

            if (!operatorNameBeforeType)
            {
                sb.Append(" ");
                if (method.IsSpecialName && _operators.ContainsKey(method.Name))
                {
                    sb.Append(_operators[method.Name]);
                }
                else
                {
                    sb.Append(method.Name);
                }
            }

            AppendGenericDeclaration(method, sb, typeReferencer);
            AppendMethodParameters(method, sb, typeReferencer);

            return sb.ToString();
        }

        public string GetSignature(FieldInfo field)
        {
            if (field == null) throw new ArgumentNullException("field");

            var typeReferencer = new CSharpTypeReferenceProvider(field.ReflectedType);
            var sb = new StringBuilder();

            if (field.IsLiteral)
            {
                sb.Append("const ");
            }
            else if (field.IsStatic)
            {
                sb.Append("static ");
            }

            if (field.IsInitOnly)
            {
                sb.Append("readonly ");
            }

            sb.Append(typeReferencer.GetTypeReference(field.FieldType));
            sb.Append(" ");
            sb.Append(field.Name);

            return sb.ToString();
        }

        public string GetSignature(EventInfo event1)
        {
            if (event1 == null) throw new ArgumentNullException("event1");

            var typeReferencer = new CSharpTypeReferenceProvider(event1.ReflectedType);
            var sb = new StringBuilder();

            if (!event1.ReflectedType.IsInterface)
            {
                var adder = event1.GetAddMethod(true);
                AppendNonAccessModifiers(adder, sb);
            }

            sb.Append("event ");
            sb.Append(typeReferencer.GetTypeReference(event1.EventHandlerType));
            sb.Append(" ");
            sb.Append(event1.Name);

            return sb.ToString();
        }

        private static bool IsDelegateType(Type type)
        {
            return type.BaseType == typeof(MulticastDelegate);
        }

        private static void AppendNonAccessModifiers(Type type, StringBuilder sb)
        {
            if (type.IsAbstract)
            {
                if (type.IsSealed)
                {
                    sb.Append("static ");
                }
                else
                {
                    sb.Append("abstract ");
                }
            }
        }

        private static void AppendNonAccessModifiers(MethodBase method, StringBuilder sb)
        {
            if (method.IsStatic)
            {
                sb.Append("static ");
            }

            if (method.IsAbstract)
            {
                sb.Append("abstract ");
            }
        }

        private static void AppendGenericDeclaration(MethodInfo method, StringBuilder sb, CSharpTypeReferenceProvider typeReferencer)
        {
            if (method.IsGenericMethod)
            {
                sb.Append('<');
                sb.Append(string.Join(", ", method.GetGenericArguments().Select(x => typeReferencer.GetTypeReference(x))));
                sb.Append('>');
            }
        }

        private static void AppendMethodParameters(MethodBase method, StringBuilder sb, CSharpTypeReferenceProvider typeReferencer)
        {
            sb.Append("(");

            if (IsExtensionMethod(method))
            {
                sb.Append("this ");
            }

            sb.Append(FormatParameters(method.GetParameters(), typeReferencer));
            sb.Append(")");
        }

        private static bool IsExtensionMethod(MethodBase method)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(method);
            return attributes.Any(x => x.ToString().Equals(ExtensionAttributeSignature));
        }

        private static string FormatParameters(IEnumerable<ParameterInfo> parameters, CSharpTypeReferenceProvider typeReferencer)
        {
            var paramStrings = parameters
                .Select(p => String.Format(
                    CultureInfo.InvariantCulture, 
                    "{0} {1}", 
                    FormatParameter(p, typeReferencer), 
                    p.Name))
                .ToArray();
            return String.Join(", ", paramStrings);
        }

        private static string FormatParameter(ParameterInfo p, CSharpTypeReferenceProvider typeReferencer)
        {
            if (p.ParameterType.IsByRef)
            {
                var elementType = typeReferencer.GetTypeReference(p.ParameterType.GetElementType());
                if (p.IsOut)
                {
                    return "out " + elementType;
                }
                else
                {
                    return "ref " + elementType;
                }
            }
            else
            {
                return typeReferencer.GetTypeReference(p.ParameterType);
            }
        }

        private static void AppendBaseTypeAndInterfaces(Type type, StringBuilder sb, CSharpTypeReferenceProvider typeReferencer)
        {
            var classBase = new List<string>();

            if (type.BaseType != null &&
                type.BaseType != typeof(object) &&
                type.BaseType != typeof(ValueType))
            {
                classBase.Add(typeReferencer.GetTypeReference(type.BaseType));
            }

            foreach (var interfaceDeclaration in type.GetInterfaces())
            {
                classBase.Add(typeReferencer.GetTypeReference(interfaceDeclaration));
            }

            if (classBase.Count > 0)
            {
                sb.Append(" : ");
                sb.Append(string.Join(", ", classBase));
            }
        }
    }
}
