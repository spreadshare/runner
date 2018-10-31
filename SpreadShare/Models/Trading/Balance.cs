using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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
        public Currency Symbol;

        /// <summary>
        /// Amount of balance that is free
        /// </summary>
        public decimal Free;

        /// <summary>
        /// Amount of balance that is locked
        /// </summary>
        public decimal Locked;

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
        
        public static Balance Empty(Currency c) => new Balance(c, 0.0M, 0.0M);
    }
}