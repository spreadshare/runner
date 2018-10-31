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
        private const decimal DustThreshold = 0.01M;


        /// <summary>
        /// Initializes a new instance of the <see cref="TotalPortfolio"/> class.
        /// </summary>
        public TotalPortfolio()
        {
            _allocations = new Dictionary<Type, AlgorithmPortfolio>();
        }

        public void AddEntry(Type algorithm, AlgorithmPortfolio allocation)
        {
            _allocations.Add(algorithm, allocation);
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
        /// <param name="remoteAssets">Remote assets to compare with</param>
        /// <returns>Whether or not the situations can be considered equal</returns>
        public bool GetDifferenceWithRemote(Assets remoteAssets)
        {
            Assets sum = null;
            foreach (var alg in _allocations.Values)
            {
                sum = alg.getAsAssets().Union(sum);
            }

            var difference = remoteAssets.Difference(sum).GetAllTotalBalances();

            foreach (var assetValue in difference)
            {
                if (assetValue.Amount < DustThreshold)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a certain algorithm is allocated.
        /// </summary>
        /// <param name="alg">The algorithm type to evaluate</param>
        /// <returns>Whether the algorithm is allocated</returns>
        public bool AllocatesAlgorithm(Type alg)
        {
            return _allocations.ContainsKey(alg);
        }

        /// <summary>
        /// Returns the allocation of a certain algorithm
        /// </summary>
        /// <param name="alg">The algorithm type to evaluate</param>
        /// <returns>The algorithm's portfolio</returns>
        public AlgorithmPortfolio GetAlgorithmAllocation(Type alg)
        {
            if (!_allocations.ContainsKey(alg))
                throw new ArgumentException($"No allocation available for {alg}");

            return _allocations[alg];
        }

        /// <summary>
        /// Set the allocation of a certain algorithm using an assets representation.
        /// </summary>
        /// <param name="alg">The algorithm type to evaluate</param>
        /// <param name="alloc">The allocation as assets</param>
        public void SetAlgorithmAllocation(Type alg, Assets alloc)
        {
            _allocations[alg] = new AlgorithmPortfolio(alloc);
        }
    }
}