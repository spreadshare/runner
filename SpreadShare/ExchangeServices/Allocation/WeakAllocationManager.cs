using System;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Provides restricted access to AccessManager
    /// </summary>
    internal class WeakAllocationManager
    {
        private readonly AllocationManager _allocationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakAllocationManager"/> class.
        /// </summary>
        /// <param name="allocationManager">Instance of AllocationManager</param>
        public WeakAllocationManager(AllocationManager allocationManager)
        {
            _allocationManager = allocationManager;
        }

        /// <summary>
        /// Check if algorithm has enough of certain currency
        /// </summary>
        /// <param name="algorithm">The algorithm that wants to execute a trade</param>
        /// <param name="currency">The currency to be sold</param>
        /// <param name="fundsToTrade">The amount to be sold of given currency</param>
        /// <returns>Returns if enough funds are present to execute the trade</returns>
        public bool CheckFunds(Type algorithm, Currency currency, decimal fundsToTrade)
        {
            return _allocationManager.CheckFunds(algorithm, currency, fundsToTrade);
        }

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="algorithm">Algorithm to get funds for</param>
        /// <param name="currency">Currency to get funds for</param>
        /// <returns>Available funds or -1 if not available</returns>
        public decimal GetAvailableFunds(Type algorithm, Currency currency)
        {
            return _allocationManager.GetAvailableFunds(algorithm, currency);
        }
    }
}
