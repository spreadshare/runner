using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;

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
        public TotalPortfolio()
        {
            _allocations = new Dictionary<Type, AlgorithmPortfolio>();
        }

        public void AddEntry(Type algorithm, AlgorithmPortfolio allocation)
        {
            _allocations.Add(algorithm, allocation);
        }

        /// <summary>
        /// Creates a branched version of the total portfolio using a trade personal
        /// </summary>
        /// <param name="trade">The trade proposal</param>
        /// <returns>A branched version of the portfolio</returns>
        public void ApplyTradeExecution(TradeExecution trade)
        {
            if (trade == null)
            {
                throw new ArgumentNullException("Parameter 'trade' should not be null");
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

        public void CompareWithExchange(Assets assets)
        {
            Assets sum = null;
            foreach (var alg in _allocations.Values)
            {
                sum = alg.getAsAssets().Combine(sum);
            }

            Assets a =sum.Intersection(assets);
        }

        public bool AllocatesAlgorithm(Type alg)
        {
            return _allocations.ContainsKey(alg);
        }

        public AlgorithmPortfolio GetAlgorithmAllocation(Type alg)
        {
            if (!_allocations.ContainsKey(alg))
                throw new ArgumentException($"No allocation available for {alg}");

            return _allocations[alg];
        }

        public void SetAlgorithmAllocation(Type alg, Assets alloc)
        {
            _allocations[alg] = new AlgorithmPortfolio(alloc);
        }
    }
}