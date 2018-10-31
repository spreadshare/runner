using System;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Interface for observing trades
    /// </summary>
    internal interface ITradeObserver
    {
        /// <summary>
        /// Trigger an update in the observer.
        /// </summary>
        /// <param name="algorithm">Algorithm that has traded</param>
        /// <param name="exchange">Specifies which exchange is used</param>
        void Update(Type algorithm, Exchange exchange);
    }
}
