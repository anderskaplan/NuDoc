namespace NuDoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helper class for common reflection-oriented tasks.
    /// </summary>
    public static class ReflectionHelper
    {
        public static bool IsDelegateType(Type type)
        {
            return type.BaseType == typeof(MulticastDelegate);
        }

        public static bool IsOperator(MethodBase method)
        {
            return method.IsSpecialName && method.Name.StartsWith("op_", StringComparison.Ordinal);
        }

        public static bool IsVisible(Type type)
        {
            return type.IsVisible;
        }

        public static bool IsTrivialMethod(MethodBase method)
        {
            if (method.DeclaringType == typeof(object))
            {
                return true;
            }

            if (method.IsVirtual)
            {
                var p = method.GetParameters();
                if ((method.Name.Equals("ToString") && p.Length == 0) ||
                    (method.Name.Equals("Equals") && p.Length == 1 && p[0].ParameterType == typeof(object)) ||
                    (method.Name.Equals("GetHashCode") && p.Length == 0))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<ConstructorInfo> GetVisibleConstructors(Type type)
        {
            return type.GetConstructors()
                .Where(x => x.IsPublic);
        }

        public static IEnumerable<PropertyInfo> GetVisibleProperties(Type type)
        {
            return type.GetProperties()
                .Where(x => x.GetAccessors()
                    .Any(acc => acc.IsPublic));
        }

        public static IEnumerable<MethodInfo> GetVisibleMethods(Type type)
        {
            return type.GetMethods()
                .Where(x => x.IsPublic && !x.IsSpecialName);
        }

        public static IEnumerable<MethodInfo> GetVisibleOperators(Type type)
        {
            return type.GetMethods()
                .Where(x => x.IsPublic && IsOperator(x));
        }

        public static IEnumerable<FieldInfo> GetVisibleFields(Type type)
        {
            return type.GetFields()
                .Where(x => x.IsPublic);
        }

        public static IEnumerable<EventInfo> GetVisibleEvents(Type type)
        {
            return type.GetEvents()
                .Where(x => x.GetAddMethod().IsPublic);
        }

        public static IEnumerable<FieldInfo> GetEnumMembers(Type type)
        {
            return type.GetFields()
                .Where(x => x.IsPublic && !x.IsSpecialName);
        }
    }
}
