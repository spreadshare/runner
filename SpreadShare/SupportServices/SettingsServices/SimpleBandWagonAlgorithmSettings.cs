namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings for the simple bandwagon algorithm
    /// </summary>
    internal class SimpleBandWagonAlgorithmSettings : AlgorithmSettings
    {
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