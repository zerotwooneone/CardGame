using System;

namespace GeneratorAttributes
{
    /// <summary>
    /// Apply to a private static readonly field within a class marked with [EnumLike].
    /// The field's name must start with '_'.
    /// The generator will create a public static readonly field of the containing class type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedEnumValueAttribute : Attribute
    {
        // No parameters needed; name and value come from the field itself.
    }
}