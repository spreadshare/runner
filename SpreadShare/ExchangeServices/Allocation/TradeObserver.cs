using System;

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
        public abstract void Update(Type algorithm);
    }
}
