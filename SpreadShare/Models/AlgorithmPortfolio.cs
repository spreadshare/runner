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
        public AlgorithmPortfolio()
        {
            _dict = null;
            throw new NotImplementedException();
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
        /// Returns a string format JSON representation of the portfolio
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_dict);
        }
    }
}