using System;

namespace SmartEnumGeneratorAttributes
{
    /// <summary>
    /// Apply to an enum definition to generate a corresponding Smart Enum class.
    /// The generated class will be partial, allowing you to add custom members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateSmartEnumAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the property in the generated Smart Enum
        /// that holds the underlying value (e.g., "Rank", "Id").
        /// Defaults to "Value" if not specified, matching the typical base class.
        /// </summary>
        public string ValuePropertyName { get; set; } = "Value";

        /// <summary>
        /// Gets or sets the desired name for the generated class.
        /// If null or empty, the enum's name (without any "Definition" or "Enum" suffix) will be used.
        /// </summary>
        public string? GeneratedClassName { get; set; }

        /// <summary>
        /// Gets or sets the desired namespace for the generated class.
        /// If null or empty, the enum's namespace will be used.
        /// </summary>
        public string? GeneratedNamespace { get; set; }
    }
}