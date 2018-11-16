using System.Collections.Generic;
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
        /// Gets or sets the exchange the algorithm uses
        /// </summary>
        public abstract Exchange Exchange { get; set; }
        
        /// <summary>
        /// Gets or sets the list of active trading pairs
        /// </summary>
        public abstract List<TradingPair> ActiveTradingPairs { get; set; }
    }
}