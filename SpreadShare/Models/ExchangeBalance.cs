namespace SpreadShare.Models
{
    /// <summary>
    /// Adapter wrapper for creating an <see cref="Assets"/> object
    /// </summary>
    internal class ExchangeBalance
    {
        /// <summary>
        /// Symbol of the asset
        /// </summary>
        public readonly Currency Symbol;

        /// <summary>
        /// Amount of balance that is free
        /// </summary>
        public readonly decimal Free;

        /// <summary>
        /// Amount of balance that is locked
        /// </summary>
        public readonly decimal Locked;

        /// <summary>
        /// Total balance
        /// </summary>
        public readonly decimal Total;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeBalance"/> class.
        /// </summary>
        /// <param name="symbol">Symbol of the asset</param>
        /// <param name="free">Amount of balance that is free</param>
        /// <param name="locked">Amount of balance that is locked</param>
        public ExchangeBalance(Currency symbol, decimal free, decimal locked)
        {
            Symbol = symbol;
            Free = free;
            Locked = locked;
            Total = free + locked;
        }
    }
}