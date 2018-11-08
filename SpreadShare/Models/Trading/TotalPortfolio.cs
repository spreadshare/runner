using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Algorithms;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Generic model for a total portfolio allocation for multiple algorithms.
    /// </summary>
    internal class TotalPortfolio
    {
        private Dictionary<Type, Portfolio> _allocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="TotalPortfolio"/> class.
        /// </summary>
        public TotalPortfolio()
        {
            _allocations = new Dictionary<Type, Portfolio>();
        }

        /// <summary>
        /// Modifies the allocation based on the executed trade.
        /// </summary>
        /// <param name="algo">The algorithm to attribute the trade to</param>
        /// <param name="trade">The trade proposal</param>
        public void ApplyTradeExecution(Type algo, TradeExecution trade)
        {
            var temp0 = trade ?? throw new ArgumentNullException(nameof(trade));
            var temp1 = algo ?? throw new ArgumentNullException(nameof(algo));

            if (!algo.IsSubclassOf(typeof(BaseAlgorithm)))
            {
                throw new ArgumentException(nameof(algo) + " must be a subclass of BaseAlgorithm");
            }

            // Algorithm should always be in _allocations
            if (!_allocations.ContainsKey(algo))
            {
                throw new ArgumentException(
                    $"{algo} did not receive any funds during allocation but is trying to change its portfolio.");
            }

            // Substract spent funds
            _allocations[algo].UpdateAllocation(trade);
        }

        /// <summary>
        /// Determines if an algorithm has any defined allocation.
        /// </summary>
        /// <param name="algo">The type of the algorithm to evaluate</param>
        /// <returns>Boolean indicating the presence of an allocation object</returns>
        public bool IsAllocated(Type algo) => _allocations.ContainsKey(algo);

        /// <summary>
        /// Returns the allocation of a certain algorithm
        /// </summary>
        /// <param name="algo">The algorithm type to evaluate</param>
        /// <returns>The algorithm's portfolio</returns>
        public Portfolio GetAlgorithmAllocation(Type algo)
        {
            var temp0 = algo ?? throw new ArgumentNullException(nameof(algo));

            if (!algo.IsSubclassOf(typeof(BaseAlgorithm)))
            {
                throw new ArgumentException(nameof(algo) + " must be a subclass of BaseAlgorithm");
            }

            if (!_allocations.ContainsKey(algo))
            {
                return Portfolio.Empty;
            }

            return _allocations[algo];
        }

        /// <summary>
        /// Set the allocation of a certain algorithm using an assets representation.
        /// </summary>
        /// <param name="algo">The algorithm type to evaluate</param>
        /// <param name="alloc">The allocation as assets</param>
        public void SetAlgorithmAllocation(Type algo, Portfolio alloc)
        {
            var temp0 = algo ?? throw new ArgumentNullException(nameof(algo));
            var temp1 = alloc ?? throw new ArgumentNullException(nameof(alloc));

            if (!algo.IsSubclassOf(typeof(BaseAlgorithm)))
            {
                throw new ArgumentException(nameof(algo) + " must be a subclass of BaseAlgorithm");
            }

            if (_allocations.ContainsKey(algo))
            {
                _allocations[algo] = alloc;
                return;
            }

            _allocations.Add(algo, alloc);
        }

        /// <summary>
        /// Returns a new portfolio containing the sum of all allocated portfolios
        /// In other words, all the assets for the particular exchange.
        /// </summary>
        /// <returns>Summed portfolio</returns>
        public Portfolio GetSummedChildren()
        {
            return _allocations.Values.Aggregate(Portfolio.Empty, Portfolio.Add);
        }
    }
}