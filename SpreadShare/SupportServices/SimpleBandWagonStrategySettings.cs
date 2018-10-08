using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Settings for the simple bandwagon strategy
    /// </summary>
    internal class SimpleBandWagonStrategySettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBandWagonStrategySettings"/> class.
        /// </summary>
        /// <param name="baseCurrency">Base currency to trade in</param>
        /// <param name="minimalRevertValue">Minimal value before reverting to the base currency</param>
        /// <param name="minimalGrowthPercentage">Minimal growth</param>
        /// TODO: Should this not be renamed to minimalPerformance?
        /// <param name="checkTime">Amount of hours to look in the past</param>
        /// <param name="holdTime">Amount of hours to hold the currency</param>
        public SimpleBandWagonStrategySettings(
            Currency baseCurrency,
            decimal minimalRevertValue,
            decimal minimalGrowthPercentage,
            int checkTime,
            int holdTime)
        {
            BaseCurrency = baseCurrency;
            MinimalRevertValue = minimalRevertValue;
            MinimalGrowthPercentage = minimalGrowthPercentage;
            CheckTime = checkTime;
            HoldTime = holdTime;
        }

        /// <summary>
        /// Gets the base currency to trade in
        /// </summary>
        public Currency BaseCurrency { get; }

        /// <summary>
        /// Gets the minimal value before reverting to the base currency
        /// </summary>
        public decimal MinimalRevertValue { get; }

        /// <summary>
        /// Gets the minimal growth
        /// </summary>
        public decimal MinimalGrowthPercentage { get; }

        /// <summary>
        /// Gets the amount of hours to look in the past
        /// </summary>
        public int CheckTime { get; }

        /// <summary>
        /// Gets the amount of hours to hold the currency
        /// </summary>
        public int HoldTime { get; }
    }
}