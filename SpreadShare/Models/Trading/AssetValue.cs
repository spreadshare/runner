namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representing the amount of an asset
    /// </summary>
    internal struct AssetValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValue"/> struct.
        /// </summary>
        /// <param name="symbol">Symbol of an asset</param>
        /// <param name="amount">Amount of an asset</param>
        public AssetValue(Currency symbol, decimal amount)
        {
            Symbol = symbol;
            Amount = amount;
        }

        /// <summary>
        /// Gets the symbol (or ticker) of an asset
        /// </summary>
        public Currency Symbol { get; }

        /// <summary>
        /// Gets the amount of an asset
        /// </summary>
        public decimal Amount { get; }
    }
}