using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Storage.Internal;

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
        /// <param name="trade">The trade proposal</param>
        public void ApplyTradeExecution(TradeExecution trade)
        {
            if (trade == null)
            {
                throw new ArgumentNullException(nameof(trade));
            }

            // Algorithm should always be in _allocations
            if (!_allocations.ContainsKey(trade.Algorithm))
            {
                throw new ArgumentException(
                    $"{trade.Algorithm} did not receive any funds during allocation but is trying to change its portfolio.");
            }

            // Substract spent funds
            _allocations[trade.Algorithm].UpdateAllocation(trade);
        }

        /// <summary>
        /// Determines of a portfolio matches the exchange report within a margin.
        /// </summary>
        /// <param name="remote">Remote assets to compare with</param>
        /// <returns>Whether or not the situations can be considered equal</returns>
        public List<Balance> GetDifferenceWithRemote(Portfolio remote)
        {
            var sum = _allocations.Values.Aggregate((a, b) => Portfolio.Add(a, b));
            return Portfolio.AbsoluteDifferences(sum, remote);
        }

        public bool IsAllocated(Type algo) => _allocations.ContainsKey(algo);

        /// <summary>
        /// Returns the allocation of a certain algorithm
        /// </summary>
        /// <param name="alg">The algorithm type to evaluate</param>
        /// <returns>The algorithm's portfolio</returns>
        public Portfolio GetAlgorithmAllocation(Type alg)
        {
            if (!_allocations.ContainsKey(alg))
            {
                return Portfolio.Empty;
            }

            return _allocations[alg];
        }

        /// <summary>
        /// Set the allocation of a certain algorithm using an assets representation.
        /// </summary>
        /// <param name="alg">The algorithm type to evaluate</param>
        /// <param name="alloc">The allocation as assets</param>
        public void SetAlgorithmAllocation(Type alg, Portfolio alloc)
        {
            if (_allocations.ContainsKey(alg))
            {
                _allocations[alg] = alloc;
                return;
            }
                
            _allocations.Add(alg, alloc);
        }
    }
}