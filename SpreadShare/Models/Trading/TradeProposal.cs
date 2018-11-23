using Dawn;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Generic model representing a trade.
    /// </summary>
    internal class TradeProposal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeProposal"/> class.
        /// </summary>
        /// <param name="pair">Used to round the balance</param>
        /// <param name="from">The asset value on the left side of the trade</param>
        public TradeProposal(TradingPair pair, Balance from)
        {
            Guard.Argument(pair).NotNull().Require(
                x => x.Left == from.Symbol || x.Right == from.Symbol,
                x => $"{from.Symbol} was not contained in {pair}, invalid proposal");
            From = pair.RoundToTradable(from);
        }

        /// <summary>
        /// Gets the left side of the proposed trade
        /// </summary>
        public Balance From { get; }
    }
}