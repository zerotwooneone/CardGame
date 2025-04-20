using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// This base class is typically placed in a shared library or generated/included
// by the source generator itself if needed. For this example, we assume it exists
// and the generated code will inherit from it.
// NOTE: This is NOT part of the Source Generator project itself, but code the
// generated output depends on. It could be in the Attributes library or a Core library.

namespace SmartEnumBase // Or your preferred namespace
{
    /// <summary>
    /// Base class for creating type-safe, smart enums.
    /// </summary>
    /// <typeparam name="TEnum">The type of the specific smart enum class inheriting from this base.</typeparam>
    /// <typeparam name="TValue">The type of the underlying value (e.g., int, string).</typeparam>
    public abstract class SmartEnum<TEnum, TValue> : IEquatable<SmartEnum<TEnum, TValue>>
        where TEnum : SmartEnum<TEnum, TValue>
        where TValue : IEquatable<TValue>, IComparable<TValue> // Added IComparable constraint
    {
        private static readonly Lazy<List<TEnum>> _list = new Lazy<List<TEnum>>(() =>
            typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(f => f.FieldType == typeof(TEnum))
                         .Select(f => (TEnum)f.GetValue(null))
                         .OrderBy(e => e.Value) // Order by the underlying value
                         .ToList());

        /// <summary>
        /// Gets the underlying value of the enum instance.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Gets the name of the enum instance (usually corresponds to the static field name).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartEnum{TEnum, TValue}"/> class.
        /// </summary>
        /// <param name="value">The underlying value.</param>
        /// <param name="name">The name.</param>
        protected SmartEnum(TValue value, string name)
        {
            // Basic validation can be added here if needed
            // if (name == null) throw new ArgumentNullException(nameof(name));

            Value = value;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets a list of all defined enum instances.
        /// </summary>
        /// <returns>A read-only list of all instances.</returns>
        public static IReadOnlyCollection<TEnum> List() => _list.Value.AsReadOnly();

        /// <summary>
        /// Gets the enum instance corresponding to the specified value.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>The matching enum instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no matching instance is found.</exception>
        public static TEnum FromValue(TValue value)
        {
            var matchingItem = List().FirstOrDefault(item => EqualityComparer<TValue>.Default.Equals(item.Value, value));
            if (matchingItem == null)
            {
                throw new InvalidOperationException($"'{value}' is not a valid value in {typeof(TEnum).Name}");
            }
            return matchingItem;
        }

        /// <summary>
        /// Attempts to get the enum instance corresponding to the specified value.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <param name="result">When this method returns, contains the enum instance, if found; otherwise, null.</param>
        /// <returns>true if a matching instance was found; otherwise, false.</returns>
        public static bool TryFromValue(TValue value, out TEnum? result)
        {
            result = List().FirstOrDefault(item => EqualityComparer<TValue>.Default.Equals(item.Value, value));
            return result != null;
        }


        /// <summary>
        /// Gets the enum instance corresponding to the specified name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <param name="ignoreCase">Whether to ignore case during matching.</param>
        /// <returns>The matching enum instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no matching instance is found.</exception>
        public static TEnum FromName(string name, bool ignoreCase = false)
        {
             var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
             var matchingItem = List().FirstOrDefault(item => string.Equals(item.Name, name, comparison));
             if (matchingItem == null)
             {
                 throw new InvalidOperationException($"'{name}' is not a valid name in {typeof(TEnum).Name}");
             }
             return matchingItem;
        }

        /// <summary>
        /// Attempts to get the enum instance corresponding to the specified name.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <param name="ignoreCase">Whether to ignore case during matching.</param>
        /// <param name="result">When this method returns, contains the enum instance, if found; otherwise, null.</param>
        /// <returns>true if a matching instance was found; otherwise, false.</returns>
        public static bool TryFromName(string name, bool ignoreCase, out TEnum? result)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            result = List().FirstOrDefault(item => string.Equals(item.Name, name, comparison));
            return result != null;
        }


        /// <summary>
        /// Returns the name of the enum instance.
        /// </summary>
        /// <returns>The name.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj) =>
            obj is SmartEnum<TEnum, TValue> other && Equals(other);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public virtual bool Equals(SmartEnum<TEnum, TValue>? other)
        {
             if (other is null)
             {
                 return false;
             }
             // If Values are default values, compare by names. Necessary for default(T) comparison.
             if (EqualityComparer<TValue>.Default.Equals(Value, default!) && EqualityComparer<TValue>.Default.Equals(other.Value, default!))
             {
                 return Name == other.Name;
             }
             return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => EqualityComparer<TValue>.Default.GetHashCode(Value);

        /// <summary>
        /// Determines whether two specified instances of <see cref="SmartEnum{TEnum, TValue}"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right are equal; otherwise, false.</returns>
        public static bool operator ==(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="SmartEnum{TEnum, TValue}"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if left and right are not equal; otherwise, false.</returns>
         public static bool operator !=(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right) =>
             !(left == right);

         // Optional: Add comparison operators if needed, comparing by Value
         public static bool operator <(SmartEnum<TEnum, TValue> left, SmartEnum<TEnum, TValue> right) =>
             left.Value.CompareTo(right.Value) < 0;

         public static bool operator <=(SmartEnum<TEnum, TValue> left, SmartEnum<TEnum, TValue> right) =>
             left.Value.CompareTo(right.Value) <= 0;

         public static bool operator >(SmartEnum<TEnum, TValue> left, SmartEnum<TEnum, TValue> right) =>
             left.Value.CompareTo(right.Value) > 0;

         public static bool operator >=(SmartEnum<TEnum, TValue> left, SmartEnum<TEnum, TValue> right) =>
             left.Value.CompareTo(right.Value) >= 0;
    }
}
