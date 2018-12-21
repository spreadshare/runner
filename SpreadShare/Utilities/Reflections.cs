using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dawn;
using SpreadShare.Algorithms;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Static utility functions regarding reflections.
    /// </summary>
    internal static class Reflections
    {
        /// <summary>
        /// Gets a one time initialized property for the assembly.
        /// </summary>
        private static Assembly ThisAssembly { get; } = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), assembly
            => assembly.ManifestModule.Name.Contains("SpreadShare.dll", StringComparison.InvariantCulture));

        /// <summary>
        /// Fetches a collection of types which are explicit subtypes of the given abstract type.
        /// </summary>
        /// <param name="abstraction">Base type.</param>
        /// <returns>All implementations of given base type.</returns>
        public static IEnumerable<Type> GetAllSubtypes(Type abstraction)
        {
            Guard.Argument(abstraction).Require(
                x => !x.IsInterface,
                x => $"{x} is an interface, please use the GetAllImplementations() method instead");
            return from type in ThisAssembly.GetTypes()
                   where type.IsSubclassOf(abstraction)
                   select type;
        }

        /// <summary>
        /// Fetches a collection of types which are explicit implementations of the given interface.
        /// </summary>
        /// <param name="abstraction">Type of the interface.</param>
        /// <returns>All implementations of a given interface.</returns>
        public static IEnumerable<Type> GetAllImplementations(Type abstraction)
        {
            Guard.Argument(abstraction).Require(
                x => x.IsInterface,
                x => $"{x} is not an interface, did you mean to call GetAllSubtypes()?");
            return from type in ThisAssembly.GetTypes()
                where type.GetInterfaces().Contains(abstraction)
                select type;
        }

        /// <summary>
        /// Check whether a type implements IBaseAlgorithm.
        /// </summary>
        /// <param name="t">Type to check.</param>
        /// <returns>Whether the provided type is an algorithm.</returns>
        public static bool IsAlgorithm(Type t)
        {
            return t.GetInterfaces().Contains(typeof(IBaseAlgorithm));
        }

        /// <summary>
        /// Get a dictionary of classes with class name.
        /// </summary>
        /// <returns>Dictionary of classes with class name.</returns>
        public static Dictionary<string, TypeInfo> GetClasses()
        {
            // Get all defined classes
            var typeInfos = ThisAssembly.DefinedTypes;
            Dictionary<string, TypeInfo> classes = new Dictionary<string, TypeInfo>();
            foreach (var typeInfo in typeInfos)
            {
                var typeInfoName = typeInfo.Name;

                // Remove noise
                string[] patterns = { "`1", "+<>c" };
                foreach (string pattern in patterns)
                {
                    if (typeInfoName.EndsWith(pattern, true, CultureInfo.InvariantCulture))
                    {
                        typeInfoName = typeInfoName.Substring(0, typeInfoName.Length - pattern.Length);
                    }
                }

                if (!classes.ContainsKey(typeInfoName))
                {
                    classes.Add(typeInfoName, typeInfo);
                }
                else
                {
                    // Make sure the algorithm class does not have different parameterized types
                    classes[typeInfoName] = null;
                }
            }

            return classes;
        }
    }
}