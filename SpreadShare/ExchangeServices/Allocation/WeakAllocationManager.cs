using System;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Provides restricted access to AccessManager by representing a certain algorithm on a certain exchange.
    /// </summary>
    internal class WeakAllocationManager
    {
        private readonly AllocationManager _allocationManager;
        private readonly Type _algorithm;
        private Exchange _exchange;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakAllocationManager"/> class.
        /// </summary>
        /// <param name="allocationManager">Instance of AllocationManager</param>
        /// <param name="algorithm">The algorithm to represent</param>
        /// <param name="exchange">The exchange to represent</param>
        public WeakAllocationManager(AllocationManager allocationManager, Type algorithm, Exchange exchange)
        {
            _allocationManager = allocationManager;
            _algorithm = algorithm;
            _exchange = exchange;
        }

        /// <summary>
        /// Check if algorithm has enough of certain currency
        /// </summary>
        /// <param name="currency">The currency to be sold</param>
        /// <param name="fundsToTrade">The amount to be sold of given currency</param>
        /// <returns>Returns if enough funds are present to execute the trade</returns>
        public bool CheckFunds(Currency currency, decimal fundsToTrade)
            => _allocationManager.CheckFunds(_exchange, _algorithm, currency, fundsToTrade);

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="currency">Currency to get funds for</param>
        /// <returns>Available funds or -1 if not available</returns>
        public Balance GetAvailableFunds(Currency currency)
            => _allocationManager.GetAvailableFunds(_exchange, _algorithm, currency);

        /// <summary>
        /// Get all available funds for a given algorithm as portfolio.
        /// </summary>
        /// <returns>Portfolio containing available funds</returns>
        public Portfolio GetAllFunds()
            => _allocationManager.GetAllFunds(_exchange, _algorithm);

        /// <summary>
        /// Queue a trade based on a proposal, the callback must return the trade execution
        /// which will be used to update the allocation.
        /// </summary>
        /// <param name="p">TradeProposal to be verified</param>
        /// <param name="tradeCallback">Trade callback to be executed if verification was succesful</param>
        /// <returns>Boolean indicating succesful execution</returns>
        public bool QueueTrade(TradeProposal p, Func<TradeExecution> tradeCallback)
            => _allocationManager.QueueTrade(p, _algorithm, _exchange, tradeCallback);
    }
}
