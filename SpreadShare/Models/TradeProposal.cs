namespace SpreadShare.Models
{
    /// <summary>
    /// Generic model representing a trade.
    /// </summary>
    internal class TradeProposal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeProposal    "/> class.
        /// </summary>
        /// <param name="pair">The currency pair</param>
        /// <param name="quantity">the amount in non base currency</param>
        /// <param name="side">buy or sell order</param>
        public TradeProposal(CurrencyPair pair, decimal quantity, OrderSide side)
        {
            Pair = pair;
            Quantity = quantity;
            Side = side;
        }

        /// <summary>
        /// Gets the currency pair of the proposed trade
        /// </summary>
        public CurrencyPair Pair { get; }

        /// <summary>
        /// Gets the quantity of the proposed trade
        /// </summary>
        public decimal Quantity { get; }

        /// <summary>
        /// Gets the order side of the proposed trade
        /// </summary>
        public OrderSide Side { get; }
    }
}