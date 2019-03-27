using System;
using Dawn;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Model for representing a currency a certain quantity, locked or free.
    /// </summary>
    internal struct Balance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Balance"/> struct.
        /// </summary>
        /// <param name="symbol">Symbol of the asset.</param>
        /// <param name="free">Quantity of the balance that is free.</param>
        /// <param name="locked">Quantity of the balance that is locked.</param>
        public Balance(Currency symbol, decimal free, decimal locked)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Free = free;
            Locked = locked;
        }

        /// <summary>
        /// Gets or sets the symbol of the asset.
        /// </summary>
        public Currency Symbol { get; set; }

        /// <summary>
        /// Gets or sets the quantity of balance that is free.
        /// </summary>
        public decimal Free { get; set; }

        /// <summary>
        /// Gets or sets the quantity of balance that is locked.
        /// </summary>
        public decimal Locked { get; set; }

        public static Balance operator -(Balance a, Balance b)
        {
            Guard.Argument(a).Require<InvalidOperationException>(
                _ => a.Symbol == b.Symbol,
                _ => $"Balances with different symbols cannot be subtracted ({a}-{b})");

            return new Balance(a.Symbol, a.Free - b.Free, a.Locked - b.Locked);
        }

        public static Balance operator +(Balance a, Balance b)
        {
            Guard.Argument(a).Require<InvalidOperationException>(
                _ => a.Symbol == b.Symbol,
                _ => $"Balances with different symbols cannot be added ({a}+{b})");

            return new Balance(a.Symbol, a.Free + b.Free, a.Locked + b.Locked);
        }

        /// <summary>
        /// Returns a new instance of balance with free and locked balances set to zero given a certain currency.
        /// </summary>
        /// <param name="c">Currency to represent.</param>
        /// <returns>A zero initiated balance object.</returns>
        public static Balance Empty(Currency c) => new Balance(c, 0.0M, 0.0M);

        /// <summary>
        /// Indicates whether this balance is contained in the given balance.
        /// </summary>
        /// <param name="other">The balance to compare to.</param>
        /// <returns>Whether the balance is contained in another balance.</returns>
        /// <exception cref="InvalidOperationException">When balances with different symbols are compared.</exception>
        public bool ContainedIn(Balance other)
        {
            if (Symbol != other.Symbol)
            {
                throw new InvalidOperationException($"Cannot compare two balances with different symbols: {this} and {other}");
            }

            if (Free <= other.Free && Locked <= other.Locked)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Symbol} -> {Free}|{Locked}";
        }
    }
}