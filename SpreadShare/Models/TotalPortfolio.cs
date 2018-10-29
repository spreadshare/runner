using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    /// <summary>
    /// Generic model for a total portfolio allocation for multiple algorithms.
    /// </summary>
    internal class TotalPortfolio
    {
        private Dictionary<Type, AlgorithmPortfolio> _allocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="TotalPortfolio"/> class.
        /// </summary>
        /// <param name="allocations">The initial allocation</param>
        public TotalPortfolio(Dictionary<Type, AlgorithmPortfolio> allocations)
        {
            _allocations = allocations;
        }

        /// <summary>
        /// Creates a branched version of the total portfolio using a trade personal
        /// </summary>
        /// <param name="trade">The trade proposal</param>
        /// <returns>A branched version of the portfolio</returns>
        public TotalPortfolio Branch(TradeProposal trade)
        {
            if (!_allocations.ContainsKey(trade.Algorithm))
            {
                throw new ArgumentException(
                    $"{trade.Algorithm} did not receive any funds during allocation but is trying to change its portfolio.");
            }

            // Create a deep copy of the dictionary
            var newAllocations = new Dictionary<Type, AlgorithmPortfolio>(_allocations);

            newAllocations[trade.Algorithm].UpdateAllocation(trade);
            return new TotalPortfolio(newAllocations);
        }
    }
}