using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Provides information about the allocated funds of an algorithm.
    /// </summary>
    internal class Portfolio
    {
        private Dictionary<Currency, Balance> _dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="Portfolio"/> class.
        /// </summary>
        /// <param name="dict">The initial values</param>
        public Portfolio(Dictionary<Currency, Balance> dict)
        {
            _dict = dict;
        }

        /// <summary>
        /// Gets a new portfolio instance with all balances set to zero.
        /// </summary>
        public static Portfolio Empty => new Portfolio(new Dictionary<Currency, Balance>());

        /// <summary>
        /// Adds to portfolio instances and returns the result
        /// </summary>
        /// <param name="first">First portfolio</param>
        /// <param name="second">Second portfolio</param>
        /// <returns>Combined portfolio</returns>
        public static Portfolio Add(Portfolio first, Portfolio second)
        {
            var foo = first._dict;
            var bar = second._dict;
            var allKeys = foo.Keys.Union(bar.Keys);
            var res = allKeys.ToDictionary(key => key, key => new Balance(
                key,
                (foo.Keys.Contains(key) ? foo[key].Free : 0.0M) + (bar.Keys.Contains(key) ? bar[key].Free : 0.0M),
                (foo.Keys.Contains(key) ? foo[key].Locked : 0.0M) + (bar.Keys.Contains(key) ? bar[key].Locked : 0.0M)));
            return new Portfolio(res);
        }

        /// <summary>
        /// Gets a copy of the Assets scaled down with given scale
        /// </summary>
        /// <param name="portfolio">The portfolio to duplicate</param>
        /// <param name="scale">Decimal between 0 and 1 indicating the scale</param>
        /// <returns>Exchange balance corresponding to the given currency</returns>
        public static Portfolio DuplicateWithScale(Portfolio portfolio, decimal scale)
        {
            if (scale < 0 || scale > 1)
            {
                throw new ArgumentException("Argument 'scale' should be between 0 and 1.");
            }

            // Create deep copy of the dictionary
            var ret = new Portfolio(new Dictionary<Currency, Balance>(portfolio._dict));

            // Map the scaling factor over all the values.
            ret = new Portfolio(ret._dict.Values.Select(
                x => new Balance(
                    x.Symbol,
                    x.Free * scale,
                    x.Locked * scale)).ToDictionary(x => x.Symbol, x => x));
            return ret;
        }

        /// <summary>
        /// Calculates the substracted difference (both free and locked) between two portfolios
        /// </summary>
        /// <param name="first">The first portfolio</param>
        /// <param name="second">The second portfolio</param>
        /// <returns>List of balances representing the differences</returns>
        public static List<Balance> SubtractedDifferences(Portfolio first, Portfolio second)
        {
            var foo = first._dict;
            var bar = second._dict;
            var allKeys = foo.Keys.Union(bar.Keys);
            var res = allKeys.Select(key => new Balance(
                key,
                (foo.Keys.Contains(key) ? foo[key].Free : 0.0M) - (bar.Keys.Contains(key) ? bar[key].Free : 0.0M),
                (foo.Keys.Contains(key) ? foo[key].Locked : 0.0M) - (bar.Keys.Contains(key) ? bar[key].Locked : 0.0M)));
            return res.Where(x => x.Free != 0.0M || x.Locked != 0.0M).ToList();
        }

        /// <summary>
        /// Returns the quantity of allocated funds, will return 0 if nothing is allocated
        /// </summary>
        /// <param name="c">Currency to query</param>
        /// <returns>Allocated funds</returns>
        public Balance GetAllocation(Currency c)
        {
            var temp0 = c ?? throw new ArgumentNullException(nameof(c));
            return _dict.GetValueOrDefault(c, Balance.Empty(c));
        }

        /// <summary>
        /// Update the allocation of an algorithm
        /// </summary>
        /// <param name="trade">The trade proposal to digest</param>
        public void UpdateAllocation(TradeExecution trade)
        {
            var temp0 = trade ?? throw new ArgumentNullException(nameof(trade));

            // TODO: Offset for dust?
            if (_dict[trade.From.Symbol].Free < trade.From.Free || _dict[trade.From.Symbol].Locked < trade.From.Locked)
            {
                // TODO: Report a critical error that shutdowns all algorithms
                throw new InvalidOperationException("Trade was invalid with respect to the allocation!");
            }

            // Substract left side of the trade
            _dict[trade.From.Symbol] -= trade.From;

            // The acquired asset can be a non entry
            if (!_dict.ContainsKey(trade.To.Symbol))
            {
                _dict.Add(trade.To.Symbol, Balance.Empty(trade.To.Symbol));
            }

            // Add right side of the trade
            _dict[trade.To.Symbol] += trade.To;
        }

        /// <summary>
        /// Returns all balances as a list
        /// </summary>
        /// <returns>list of balances</returns>
        public IEnumerable<Balance> AllBalances()
        {
            return _dict.Values;
        }

        /// <summary>
        /// Returns a string format JSON representation of the portfolio
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_dict.Where(
                pair => pair.Value.Free + pair.Value.Locked > 0.0M));
        }
    }
}