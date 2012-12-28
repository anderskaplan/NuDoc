namespace NuDoc
{
    using System;
    using System.Reflection;

    public interface ILanguageSignatureProvider
    {
        /// <summary>
        /// Gets the full name of a type, with namespace and all.
        /// </summary>
        string GetFullName(Type type);

        /// <summary>
        /// Gets the display name of a type, without namespace and other details.
        /// </summary>
        string GetDisplayName(Type type);

        /// <summary>
        /// Gets the meta-type name of a type, for example "class" or "enum".
        /// </summary>
        string GetMetaTypeName(Type type);

        /// <summary>
        /// Gets the signature for a type.
        /// </summary>
        string GetSignature(Type type);

        /// <summary>
        /// Gets the signature for a constructor.
        /// </summary>
        string GetSignature(ConstructorInfo constructor);

        /// <summary>
        /// Gets the signature for a property.
        /// </summary>
        string GetSignature(PropertyInfo property);

        /// <summary>
        /// Gets the signature for a method.
        /// </summary>
        string GetSignature(MethodInfo method);

        /// <summary>
        /// Gets the signature for a field.
        /// </summary>
        string GetSignature(FieldInfo field);

        /// <summary>
        /// Gets the signature for an event.
        /// </summary>
        string GetSignature(EventInfo event1);
    }
}
