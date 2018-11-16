using System.Collections.Generic;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings for the simple bandwagon algorithm
    /// </summary>
    internal class SimpleBandWagonAlgorithmSettings : AlgorithmSettings
    {
        /// <inheritdoc />
        public override Exchange Exchange { get; set; }

        /// <summary>
        /// Gets or sets the trading pairs for this algorithm.
        /// </summary>
        public override List<TradingPair> ActiveTradingPairs { get; set; }

        /// <summary>
        /// Gets or sets the base currency to trade in
        /// </summary>
        public Currency BaseCurrency { get; set; }

        /// <summary>
        /// Gets or sets the minimal value before reverting to the base currency
        /// </summary>
        public decimal MinimalRevertValue { get; set; }

        /// <summary>
        /// Gets or sets the minimal growth
        /// </summary>
        public decimal MinimalGrowthPercentage { get; set; }

        /// <summary>
        /// Gets or sets the amount of hours to look in the past
        /// </summary>
        public uint CheckTime { get; set; }

        /// <summary>
        /// Gets or sets the amount of hours to hold the currency
        /// </summary>
        public uint HoldTime { get; set; }
    }
}