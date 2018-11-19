using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings for backtesting containing the portfolio and the timerange of the data.
    /// </summary>
    internal class BacktestSettings
    {
        /// <summary>
        /// Gets or sets the initial backtesting portfolio.
        /// </summary>
        public Portfolio InitialPortfolio { get; set; }

        /// <summary>
        /// Gets or sets the begin timestamp of testable timerange.
        /// </summary>
        public long BeginTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the end timestamp of testable timerange.
        /// </summary>
        public long EndTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the output folder for the OutputLogger.
        /// </summary>
        public string OutputFolder { get; set; }
    }
}