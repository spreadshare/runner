using System;
using Dawn;
using Newtonsoft.Json;
using SpreadShare.Models.Serializers;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a currency.
    /// </summary>
    [JsonConverter(typeof(CurrencySerializer))]
    internal class Currency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Currency"/> class.
        /// </summary>
        /// <param name="symbol">The symbol of a currency.</param>
        public Currency(string symbol)
        {
            Guard.Argument(symbol).NotNull().NotEmpty().NotWhiteSpace();
            Symbol = symbol.ToUpperInvariant().Trim();
        }

        /// <summary>
        /// Gets the symbol of a currency
        /// TODO: Should this not be named ticker?.
        /// </summary>
        public string Symbol { get; }

        public static bool operator ==(Currency a, Currency b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        public static bool operator !=(Currency a, Currency b)
        {
            if (a is null && b is null)
            {
                return false;
            }

            if (a is null || b is null)
            {
                return true;
            }

            return !a.ToString().Equals(b.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// String representation of a currency.
        /// </summary>
        /// <returns>Returns the string representation of a currency.</returns>
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