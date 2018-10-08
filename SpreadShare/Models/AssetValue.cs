namespace SpreadShare.Models
{
    /// <summary>
    /// Object representing the value of an asset
    /// </summary>
    internal struct AssetValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValue"/> struct.
        /// </summary>
        /// <param name="symbol">Symbol of an asset</param>
        /// <param name="value">Value of an asset</param>
        public AssetValue(string symbol, decimal value)
        {
            Symbol = symbol;
            Value = value;
        }

        /// <summary>
        /// Gets the symbol (or ticker) of an asset
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the value of an asset
        /// </summary>
        /// TODO: Should this not be amount instead of value? Or is this expressed in euro's/base currency?
        public decimal Value { get; }
    }
}