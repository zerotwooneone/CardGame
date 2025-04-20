using System;

namespace SmartEnumGeneratorAttributes;

/// <summary>
/// Apply to a partial class to indicate it should be treated as a generated Smart Enum.
/// The generator will add members based on fields marked with [GeneratedEnumValue].
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EnumLikeAttribute : Attribute
{
    // Could add parameters for namespace/name overrides if needed later
}