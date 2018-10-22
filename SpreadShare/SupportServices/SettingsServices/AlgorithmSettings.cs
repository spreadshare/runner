using SpreadShare.ExchangeServices;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Abstract wrapper for algorithm settings.
    /// </summary>
    internal abstract class AlgorithmSettings
    {
        /// <summary>
        /// Gets or sets the exchange the algorithm uses
        /// </summary>
        public abstract Exchange Exchange { get; set; }
    }
}