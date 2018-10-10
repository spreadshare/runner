using System.ComponentModel;

namespace SpreadShare.Models
{
    /// <summary>
    /// Adapter wrapper for creating an <see cref="Assets"/> object
    /// </summary>
    public class ExchangeBalance
    {
        public readonly string Symbol;
        public readonly decimal Free;
        public readonly decimal Locked;
        public readonly decimal Total;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeBalance"/> class.
        /// </summary>
        public ExchangeBalance(string symbol, decimal free, decimal locked)
        {
            Symbol = symbol;
            Free = free;
            Locked = locked;
            Total = free + locked;
        }
    }
}