namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Adapter wrapper for creating an <see cref="Assets"/> object
    /// </summary>
    internal class Balance
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
        /// Initializes a new instance of the <see cref="Balance"/> class.
        /// </summary>
        /// <param name="symbol">Symbol of the asset</param>
        /// <param name="free">Amount of balance that is free</param>
        /// <param name="locked">Amount of balance that is locked</param>
        public Balance(Currency symbol, decimal free, decimal locked)
        {
            Symbol = symbol;
            Free = free;
            Locked = locked;
        }
    }
}