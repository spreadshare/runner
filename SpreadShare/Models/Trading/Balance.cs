using System;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Model for representing a currency a certain quantity, locked or free.
    /// </summary>
    internal struct Balance
    {
        /// <summary>
        /// Symbol of the asset
        /// </summary>
        public readonly Currency Symbol;

        /// <summary>
        /// Amount of balance that is free
        /// </summary>
        public decimal Free;

        /// <summary>
        /// Amount of balance that is locked
        /// </summary>
        public decimal Locked;

        /// <summary>
        /// Initializes a new instance of the <see cref="Balance"/> struct.
        /// </summary>
        /// <param name="symbol">Symbol of the asset</param>
        /// <param name="free">Amount of balance that is free</param>
        /// <param name="locked">Amount of balance that is locked</param>
        public Balance(Currency symbol, decimal free, decimal locked)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Free = free;
            Locked = locked;
        }

        public static Balance operator -(Balance a, Balance b)
        {
            if (!(a.Symbol == b.Symbol))
            {
                throw new InvalidOperationException("Balances with different symbols cannot be subtracted");
            }

            return new Balance(a.Symbol, a.Free - b.Free, a.Locked - b.Locked);
        }

        public static Balance operator +(Balance a, Balance b)
        {
            if (!(a.Symbol == b.Symbol))
            {
                throw new InvalidOperationException("Balances with different symbols cannot be added");
            }

            return new Balance(a.Symbol, a.Free + b.Free, a.Locked + b.Locked);
        }

        /// <summary>
        /// Returns a new instance of balance with free and locked balances set to zero given a certain currency.
        /// </summary>
        /// <param name="c">Currency to represent</param>
        /// <returns>A zero initiated balance object</returns>
        public static Balance Empty(Currency c) => new Balance(c, 0.0M, 0.0M);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Symbol} -> {Free}|{Locked}";
        }
    }
}