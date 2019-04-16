using System;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// Interface defining an allocation manager.
    /// </summary>
    internal interface IAllocationManager : IObservable<Portfolio>
    {
        /// <summary>
        /// Sets initial configuration of allocations per algorithm.
        /// </summary>
        /// <param name="initialAllocation">Initial set of allocations.</param>
        void SetInitialConfiguration(Portfolio initialAllocation);

        /// <summary>
        /// Gives the entire portfolio of a certain algorithm on a certain exchange.
        /// </summary>
        /// <returns>Portfolio containing all available funds.</returns>
        Portfolio GetAllFunds();

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="currency">Currency to get funds for.</param>
        /// <returns>Available funds or -1 if not available.</returns>
        Balance GetAvailableFunds(Currency currency);

        /// <summary>
        /// Updates the allocation of a given algorithm, on a certain exchange given a trade execution.
        /// </summary>
        /// <param name="exec">The trade execution to process.</param>
        void UpdateAllocation(TradeExecution exec);

        /// <summary>
        /// Queue a trade based on a proposal, the callback must return the trade execution
        /// which will be used to update the allocation.
        /// </summary>
        /// <param name="p">TradeProposal to be verified.</param>
        /// <param name="tradeCallback">Trade callback to be executed if verification was successful.</param>
        /// <returns>Boolean indicating successful execution of the callback.</returns>
        ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Func<OrderUpdate> tradeCallback);
    }
}