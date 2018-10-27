using System;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Abstract class for observing trades
    /// </summary>
    internal abstract class TradeObserver
    {
        /// <summary>
        /// Trigger an update in the observer.
        /// </summary>
        /// <param name="algorithm">Algorithm that has traded</param>
        /// <param name="exchangeSpecification">Specifies which exchange is used</param>
        public abstract void Update(Type algorithm, IExchangeSpecification exchangeSpecification);
    }
}
