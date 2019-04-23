using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using SpreadShare.Utilities;
using YamlDotNet.Serialization;

namespace SpreadShare.SupportServices.Configuration
{
    /// <summary>
    /// Utility class for validating settings.
    /// </summary>
    internal static class ConfigurationValidator
    {
        /// <summary>
        /// Throws <see cref="InvalidConfigurationException"/> when the given objects does not pass all constraints.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <exception cref="InvalidConfigurationException">Not all constraints passed.</exception>
        public static void ValidateConstraintsRecursively(object obj)
        {
            if (GetConstraintFailuresRecursively(obj).Any())
            {
                throw new InvalidConfigurationException(GetConstraintFailuresRecursively(obj).Join(Environment.NewLine));
            }
        }

        /// <summary>
        /// Validate that all properties abide by there <see cref="Constraint"/> attributes.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <returns>Description of every constraint violation.</returns>
        public static IEnumerable<string> GetConstraintFailuresRecursively(object obj)
        {
            // Base case
            if (obj == null)
            {
                yield break;
            }

            // Check write-able properties
            Type objType = obj.GetType();
            foreach (var p in objType.GetProperties())
            {
                object value = null;
                string evalError = null;
                try
                {
                    // Attempt to evaluate the value
                    value = p.GetValue(obj, null);
                }
                catch (Exception e)
                {
                    // Exception during evaluation
                    evalError = e.Unpack().Message;
                }

                if (evalError != null)
                {
                    // Yield exception and continue.
                    yield return evalError;
                    continue;
                }

                foreach (var failure in GetPropertyFailures(p, value))
                {
                    yield return failure;
                }

                if (value is IEnumerable enumerable)
                {
                    // The value is a list, recurse if it is not a simple type.
                    foreach (var child in enumerable)
                    {
                        if (!IsSimpleType(child.GetType()) && (p.CanWrite || p.GetCustomAttribute<ForceEval>() != null))
                        {
                            foreach (var failure in GetConstraintFailuresRecursively(child))
                            {
                                yield return failure;
                            }
                        }
                    }
                }
                else if (!IsSimpleType(p.PropertyType) && (p.CanWrite || p.GetCustomAttribute<ForceEval>() != null))
                {
                    foreach (var failure in GetConstraintFailuresRecursively(value))
                    {
                        yield return failure;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all constraint violations of a property given a value. (will not recurse).
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <param name="value">The value to check against.</param>
        /// <returns>All constraint violations.</returns>
        public static IEnumerable<string> GetPropertyFailures(PropertyInfo property, object value)
        {
            var constraints = property.GetCustomAttributes().OfType<Constraint>();
            foreach (var constraint in constraints)
            {
                // Skip null values unless it concerns the Required constraint.
                if (value is null && constraint.GetType() != typeof(Required))
                {
                   continue;
                }

                // If possible, use the YamlAlias as name
                string name = property.GetCustomAttribute<YamlMemberAttribute>()?.Alias ?? property.Name;
                foreach (var failure in constraint.Validate(name, value))
                {
                    yield return $"{constraint.GetType().Name}: {failure}";
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