using System.Collections.Generic;

namespace SpreadShare.Models
{
    /// <summary>
    /// Provides information about the allocated funds of an algorithm.
    /// </summary>
    internal class AlgorithmPortfolio
    {
        private Dictionary<Currency, decimal> _dict;

        public decimal GetAllocation(Currency c)
        {
            return _dict.GetValueOrDefault(c, 0.0M);
        }
    }
}