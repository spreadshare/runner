using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using YamlDotNet.Serialization;

namespace SpreadShare.SupportServices.Configuration
{
    /// <summary>
    /// Utility class for validating settings.
    /// </summary>
    internal static class ConfigurationValidator
    {
        /// <summary>
        /// Validate that all properties abide by there <see cref="Constraint"/> attributes.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        public static void ValidateConstraintsRecursively(object obj)
        {
            // Base case
            if (obj == null)
            {
                return;
            }

            Type objType = obj.GetType();
            foreach (var p in objType.GetProperties().Where(x => x.CanWrite))
            {
                // Always validate the value.
                var value = p.GetValue(obj, null);

                ValidateProperty(p, value);
                if (value is IEnumerable enumerable)
                {
                    // The value is a list, recurse if it is not a simple type.
                    foreach (var child in enumerable)
                    {
                        if (!IsSimpleType(child.GetType()))
                        {
                            ValidateConstraintsRecursively(child);
                        }
                    }
                }
                else if (!IsSimpleType(p.PropertyType))
                {
                    ValidateConstraintsRecursively(value);
                }
            }
        }

        private static void ValidateProperty(PropertyInfo property, object value)
        {
            var constraints = property.GetCustomAttributes().OfType<Constraint>();
            foreach (var constraint in constraints)
            {
                // Skip null values unless it concerns the Required constraint.
                if (value is null && constraint.GetType() != typeof(Required))
                {
                   continue;
                }

                if (!constraint.Valid(value))
                {
                    // If possible, use the YamlAlias as name
                    string name = property.GetCustomAttribute<YamlMemberAttribute>()?.Alias ?? property.Name;
                    throw new InvalidConfigurationException(
                        $"{constraint.GetType().Name}: {constraint.OnError(name, value)}");
                }
            }
        }

        /// <summary>
        /// Defines a predicate that decides whether a type is just a simple type (and therefore a leaf in the
        /// constraint validation tree, or in fact a class that contains more properties.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Whether the type is a simple type.</returns>
        private static bool IsSimpleType(Type type)
        {
            return
                type.IsValueType
                || type.IsPrimitive
                || new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid),
                }.Contains(type)
                || Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}