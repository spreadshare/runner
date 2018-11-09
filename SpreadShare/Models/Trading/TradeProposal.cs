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
        /// <param name="from">The asset value on the left side of the trade</param>
        public TradeProposal(Balance from)
        {
            From = from;
        }

        /// <summary>
        /// Gets the left side of the proposed trade
        /// </summary>
        public Balance From { get; }
    }
}