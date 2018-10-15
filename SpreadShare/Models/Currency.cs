namespace SpreadShare.Models
{
    /// <summary>
    /// Object representation of a currency
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of a currency</param>
        public Currency(string symbol) => Symbol = symbol;

        /// <summary>
        /// Gets the symbol of a currency
        /// TODO: Should this not be named ticker?
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// String representation of a currency
        /// </summary>
        /// <returns>Returns the string representation of a currency</returns>
        public override string ToString()
        {
            return Symbol;
        }
    }
}