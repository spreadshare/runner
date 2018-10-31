using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpreadShare.Models
{
    /// <summary>
    /// Provides information about the allocated funds of an algorithm.
    /// </summary>
    internal class AlgorithmPortfolio
    {
        private Dictionary<Currency, decimal> _dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmPortfolio"/> class.
        /// </summary>
        public AlgorithmPortfolio(Assets assets)
        {
            _dict = new Dictionary<Currency, decimal>();
            foreach (var assetValue in assets.GetAllFreeBalances())
            {
                _dict.Add(assetValue.Symbol, assetValue.Amount);
            }
        }

        /// <summary>
        /// Returns the quantity of allocated funds, will return 0 if nothing is allocated
        /// </summary>
        /// <param name="c">Currency pair to query</param>
        /// <returns>Allocated funds</returns>
        public decimal GetAllocation(Currency c)
        {
            return _dict.GetValueOrDefault(c, 0.0M);
        }

        /// <summary>
        /// Update the allocation of an algorithm
        /// </summary>
        /// <param name="trade">The trade proposal to digest</param>
        public void UpdateAllocation(TradeExecution trade)
        {
            //TODO: Offset for dust?
            if (_dict[trade.From.Symbol] < trade.From.Amount)
            {
                // TODO: Report a critical error that shutdowns all algorithms
                throw new ArgumentException("Trade proposal was invalid with respect to the allocation!");
            }

            _dict[trade.From.Symbol] -= trade.From.Amount;
            _dict[trade.To.Symbol] += trade.To.Amount;
        }

        /// <summary>
        /// Returns a string format JSON representation of the portfolio
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_dict);
        }

        /// <summary>
        /// Return an assets representation of this instance
        /// </summary>
        /// <returns>Assets representation</returns>
        public Assets getAsAssets()
        {
            throw new NotImplementedException();
        }
    }
}