using System;
using System.IO;
using System.Reflection;
using Dawn;
using SpreadShare.Utilities;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SpreadShare.SupportServices.Configuration
{
    /// <summary>
    /// Provides utility functions for loading settings.
    /// </summary>
    internal static class ConfigurationLoader
    {
        /// <summary>
        /// Loads a configuration.
        /// </summary>
        /// <param name="type">the type of configuration.</param>
        /// <returns>Loaded algorithm configuration.</returns>
        public static AlgorithmConfiguration LoadConfiguration(Type type)
        {
            Guard.Argument(type).Require(
                Reflections.IsAlgorithmConfiguration,
                x => $"{x} is not an algorithm configuration.");

            try
            {
                return typeof(ConfigurationLoader)
                    .GetMethod(
                        name: nameof(LoadConfiguration),
                        bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                        binder: null,
                        types: Array.Empty<Type>(),
                        modifiers: Array.Empty<ParameterModifier>())
                    .MakeGenericMethod(type)
                    .Invoke(null, null) as AlgorithmConfiguration;
            }
            catch (TargetInvocationException e)
            {
                var inner = e.InnerException;
                if (inner is YamlException)
                {
                    throw inner.InnerException;
                }

                throw inner;
            }
        }

        /// <summary>
        /// Load a configuration given a filename.
        /// </summary>
        /// <param name="type">The type of configuration.</param>
        /// <param name="filename">the name of the configuration file.</param>
        /// <returns>Loaded algorithm configuration.</returns>
        public static AlgorithmConfiguration LoadConfiguration(Type type, string filename)
        {
            Guard.Argument(type).Require(
                Reflections.IsAlgorithmConfiguration,
                x => $"{x} is not an algorithm configuration.");

            try
            {
                return typeof(ConfigurationLoader)
                    .GetMethod(
                        name: nameof(LoadConfiguration),
                        bindingAttr: BindingFlags.Static | BindingFlags.NonPublic,
                        binder: null,
                        types: new[] { typeof(string) },
                        modifiers: Array.Empty<ParameterModifier>())
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { filename }) as AlgorithmConfiguration;
            }
            catch (TargetInvocationException e)
            {
                var inner = e.InnerException;
                if (inner is YamlException)
                {
                    throw inner.InnerException;
                }

                throw inner;
            }
        }

        /// <summary>
        /// Loads a configuration using the default name.
        /// </summary>
        /// <typeparam name="T">The implementation of Algorithm configuration.</typeparam>
        /// <returns>Loaded algorithm configuration.</returns>
        private static T LoadConfiguration<T>()
            where T : AlgorithmConfiguration
        {
            string filename = Reflections.GetMatchingAlgorithmType(typeof(T)).Name + ".yaml";
            return LoadConfiguration<T>(filename);
        }

        /// <summary>
        /// Loads a configuration using a given filename.
        /// </summary>
        /// <param name="filename">The location of the file.</param>
        /// <typeparam name="T">The implementation of Algorithm Configuration.</typeparam>
        /// <returns>Loaded algorithm configuration.</returns>
        private static T LoadConfiguration<T>(string filename)
            where T : AlgorithmConfiguration
        {
            using (var file = new StreamReader(filename))
            {
                var ret = new DeserializerBuilder().Build().Deserialize<T>(file);
                ConfigurationValidator.ValidateConstraintsRecursively(ret);
                return ret;
            }
        }
    }
}