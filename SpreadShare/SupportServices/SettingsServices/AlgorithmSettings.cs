using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Abstract wrapper for algorithm settings.
    /// </summary>
    internal abstract class AlgorithmSettings
    {
        /// <summary>
        /// Gets or sets the exchange the algorithm uses.
        /// </summary>
        public Exchange Exchange { get; set; }

        /// <summary>
        /// Gets or sets the base currency the algorithm uses.
        /// </summary>
        public Currency BaseCurrency { get; set; }

        /// <summary>
        /// Gets or sets the list of active trading pairs.
        /// </summary>
        public List<TradingPair> ActiveTradingPairs { get; set; }

        /// <summary>
        /// Validates that all public properties are found in the configuration.
        /// </summary>
        /// <param name="algorithmName">Name of the algorithm as found in the configuration.</param>
        /// <param name="section">The configuration object.</param>
        public void ValidateAllSet(string algorithmName, IConfiguration section)
        {
            foreach (var property in GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                object temp = section.GetSection(
                                  $"{algorithmName}:{property.Name}").Get(property.PropertyType)
                    ?? throw new InvalidDataException(
                                  $"{algorithmName}:{property.Name} was not present the the configuration");
            }
        }
    }
}