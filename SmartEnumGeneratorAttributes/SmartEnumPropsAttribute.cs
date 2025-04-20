using System;

namespace SmartEnumGeneratorAttributes
{
    /// <summary>
    /// Apply to an enum member to specify additional properties
    /// for the corresponding static instance in the generated Smart Enum class.
    /// Property names MUST match the constructor parameter names of the generated class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SmartEnumPropsAttribute : Attribute
    {
        /// <summary>
        /// Stores property values provided via the constructor.
        /// We use an object array because attribute parameters must be constant expressions.
        /// The generator will need to map these positional arguments to constructor parameters.
        /// </summary>
        public object[] PropertyValues { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartEnumPropsAttribute"/> class.
        /// </summary>
        /// <param name="propertyValues">
        /// Values for the generated Smart Enum's constructor parameters,
        /// AFTER the standard 'value' and 'name' parameters.
        /// The order MUST match the expected order of additional parameters in the generated constructor.
        /// </param>
        public SmartEnumPropsAttribute(params object[] propertyValues)
        {
            PropertyValues = propertyValues ?? new object[0];
        }
    }
}