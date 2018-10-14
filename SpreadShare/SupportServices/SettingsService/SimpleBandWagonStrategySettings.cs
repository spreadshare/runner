using System.Collections.Generic;
using SpreadShare.Models;

namespace SpreadShare.SupportServices.SettingsService
{
    /// <summary>
    /// Settings for the simple bandwagon strategy
    /// </summary>
    internal class SimpleBandWagonStrategySettings : StrategySettings
    {      
        /// <summary>
        /// Gets the list of active trading pairs
        /// </summary>
        public List<CurrencyPair> ActiveTradingPairs { get; set; }

        /// <summary>
        /// Gets the base currency to trade in
        /// </summary>
        public Currency BaseCurrency { get; set; }

        /// <summary>
        /// Gets the minimal value before reverting to the base currency
        /// </summary>
        public decimal MinimalRevertValue { get; set; }

        /// <summary>
        /// Gets the minimal growth
        /// </summary>
        public decimal MinimalGrowthPercentage { get; set; }

        /// <summary>
        /// Gets the amount of hours to look in the past
        /// </summary>
        public uint CheckTime { get; set; }

        /// <summary>
        /// Gets the amount of hours to hold the currency
        /// </summary>
        public uint HoldTime { get; set; }
    }
}