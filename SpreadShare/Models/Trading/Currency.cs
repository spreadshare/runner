using System;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a currency
    /// </summary>
    internal class Currency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of a currency</param>
        public Currency(string symbol) => Symbol = symbol ?? throw new ArgumentException("Currency symbol can't be null");

        /// <summary>
        /// Gets the symbol of a currency
        /// TODO: Should this not be named ticker?
        /// </summary>
        private string Symbol { get; }

        public static bool operator !=(Currency a, Currency b)
        {
            return !a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        public static bool operator ==(Currency a, Currency b)
        {
            return a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// String representation of a currency
        /// </summary>
        /// <returns>Returns the string representation of a currency</returns>
        public override string ToString() => Symbol;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Currency currency && Symbol == currency.Symbol;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Symbol);
        }
    }
}