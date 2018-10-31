using System;
using System.Runtime.CompilerServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Provides restricted access to AccessManager
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
        public WeakAllocationManager(AllocationManager allocationManager, Type algorithm, Exchange exchange)
        {
            _allocationManager = allocationManager;
            _algorithm = algorithm;
            _exchange = exchange;
        }

        /// <summary>
        /// Check if algorithm has enough of certain currency
        /// </summary>
        /// <param name="exchange">Exchange to trade on</param>
        /// <param name="algorithm">The algorithm that wants to execute a trade</param>
        /// <param name="currency">The currency to be sold</param>
        /// <param name="fundsToTrade">The amount to be sold of given currency</param>
        /// <returns>Returns if enough funds are present to execute the trade</returns>
        public bool CheckFunds(Currency currency, decimal fundsToTrade)
            => _allocationManager.CheckFunds(_exchange, _algorithm, currency, fundsToTrade);

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="exchange">Exchange to trade on</param>
        /// <param name="algorithm">Algorithm to get funds for</param>
        /// <param name="currency">Currency to get funds for</param>
        /// <returns>Available funds or -1 if not available</returns>
        public decimal GetAvailableFunds(Currency currency)
            => _allocationManager.GetAvailableFunds(_exchange, _algorithm, currency);

        /// <summary>
        /// Trigger a portfolio update in the AllocationManager.
        /// </summary>
        /// <param name="algorithm">Algorithm that has traded</param>
        /// <param name="exchangeSpecification">Specifies which exchange is used</param>
        public void Update(Type algorithm, IExchangeSpecification exchangeSpecification)
            => _allocationManager.Update(algorithm, exchangeSpecification);

        /// <summary>
        /// Queue a trade based on a proposal, the callback must return the trade execution
        /// which will be used to update the allocation.
        /// </summary>
        /// <param name="p">TradeProposal to be verified</param>
        /// <param name="tradeCallback">Trade callback to be executed if verification was succesful</param>
        public bool QueueTrade(TradeProposal p, Func<TradeExecution> tradeCallback)
            => _allocationManager.QueueTrade(p, _algorithm, tradeCallback);
    }
}
