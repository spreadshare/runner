using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Newtonsoft.Json;

namespace SpreadShare.Models.Trading
{
    #pragma warning disable SA1402
    /// <summary>
    /// Provides information about the allocated funds of an algorithm.
    /// </summary>
    [JsonConverter(typeof(PortfolioSerializer))]
    internal class Portfolio
    {
        private readonly Dictionary<Currency, Balance> _dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="Portfolio"/> class.
        /// </summary>
        /// <param name="dict">The initial values.</param>
        public Portfolio(Dictionary<Currency, Balance> dict)
        {
            Guard.Argument(dict, nameof(dict)).NotNull();
            _dict = dict;
        }

        /// <summary>
        /// Gets a new portfolio instance with all balances set to zero.
        /// </summary>
        public static Portfolio Empty => new Portfolio(new Dictionary<Currency, Balance>());

        /// <summary>
        /// Adds two portfolio instances and returns the result.
        /// </summary>
        /// <param name="left">The left portfolio.</param>
        /// <param name="right">The right portfolio.</param>
        /// <returns>summed portfolio.</returns>
        public static Portfolio operator +(Portfolio left, Portfolio right)
        {
            Guard.Argument(left).NotNull();
            Guard.Argument(right).NotNull();
            var firstDict = left._dict;
            var secondDict = right._dict;
            var allKeys = firstDict.Keys.Union(secondDict.Keys);
            var res = allKeys.ToDictionary(key => key, key => new Balance(
                key,
                (firstDict.Keys.Contains(key) ? firstDict[key].Free : 0.0M) + (secondDict.Keys.Contains(key) ? secondDict[key].Free : 0.0M),
                (firstDict.Keys.Contains(key) ? firstDict[key].Locked : 0.0M) + (secondDict.Keys.Contains(key) ? secondDict[key].Locked : 0.0M)));
            return new Portfolio(res);
        }

        public static Portfolio operator -(Portfolio left, Portfolio right)
        {
            Guard.Argument(left).NotNull();
            Guard.Argument(right).NotNull();
            var foo = left._dict;
            var bar = right._dict;
            var allKeys = foo.Keys.Union(bar.Keys);
            var res = allKeys.Select(key => new Balance(
                key,
                (foo.Keys.Contains(key) ? foo[key].Free : 0.0M) - (bar.Keys.Contains(key) ? bar[key].Free : 0.0M),
                (foo.Keys.Contains(key) ? foo[key].Locked : 0.0M) - (bar.Keys.Contains(key) ? bar[key].Locked : 0.0M)));
            var dict = res.Where(x => x.Free != 0.0M || x.Locked != 0.0M).ToDictionary(x => x.Symbol, x => x);
            return new Portfolio(dict);
        }

        /// <summary>
        /// Determines whether this portfolio is contained in another portfolio.
        /// </summary>
        /// <param name="other">Portfolio to compare.</param>
        /// <returns>Whether the portfolio is contained in the other portfolio.</returns>
        public bool ContainedIn(Portfolio other)
            => AllBalances().All(x => x.ContainedIn(other.GetAllocation(x.Symbol)));

        /// <summary>
        /// Returns a deep copy of the portfolio instance.
        /// </summary>
        /// <returns>A portfolio.</returns>
        public Portfolio Clone()
            => new Portfolio(new Dictionary<Currency, Balance>(_dict));

        /// <summary>
        /// Returns the quantity of allocated funds, will return 0 if nothing is allocated.
        /// </summary>
        /// <param name="c">Currency to query.</param>
        /// <returns>Allocated funds.</returns>
        public Balance GetAllocation(Currency c)
        {
            Guard.Argument(c).NotNull();

            return _dict.GetValueOrDefault(c, Balance.Empty(c));
        }

        /// <summary>
        /// Update the allocation of an algorithm.
        /// </summary>
        /// <param name="trade">The trade proposal to digest.</param>
        public void UpdateAllocation(TradeExecution trade)
        {
            Guard.Argument(trade).NotNull();

            if (!_dict.ContainsKey(trade.From.Symbol))
            {
                _dict.Add(trade.From.Symbol, Balance.Empty(trade.From.Symbol));
            }

            // Subtract left side of the trade
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
        /// Returns all balances as a list.
        /// </summary>
        /// <returns>list of balances.</returns>
        public IEnumerable<Balance> AllBalances()
        {
            return _dict.Values;
        }
    }

    /// <summary>
    /// Custom serializer for the <see cref="Portfolio"/> model.
    /// </summary>
    internal class PortfolioSerializer : JsonConverter
    {
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var portfolio = value as Portfolio ?? Portfolio.Empty;
            serializer.Serialize(writer, portfolio.AllBalances().Where(x => x.Free != 0 || x.Locked != 0));
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var balances = serializer.Deserialize<Balance[]>(reader);
            return new Portfolio(balances.ToDictionary(x => x.Symbol, x => x));
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Portfolio);
        }
    }

    #pragma warning restore SA1402
}