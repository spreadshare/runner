namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Return value of websockets
    /// </summary>
    internal class OrderUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderUpdate"/> class.
        /// </summary>
        /// <param name="setPrice">Price at which the order was set</param>
        /// <param name="side">Side of the order</param>
        /// <param name="pair">The pair of order</param>
        /// <param name="amount">The quantity of the order</param>
        /// <param name="orderId">The unique id of the order</param>
        public OrderUpdate(
            decimal setPrice,
            OrderSide side,
            TradingPair pair,
            decimal amount,
            long orderId)
        {
            SetPrice = setPrice;
            Side = side;
            Status = OrderStatus.New;
            Pair = pair;
            Amount = amount;
            OrderId = orderId;
            SetPrice = setPrice;
            TotalFilled = 0;
            LastFillIncrement = 0;
            LastFillPrice = 0;
        }

        /// <summary>
        /// The status of an order
        /// </summary>
        public enum OrderStatus
        {
            /// <summary>
            /// A new order
            /// </summary>
            New,

            /// <summary>
            /// A partially filled order
            /// </summary>
            PartiallyFilled,

            /// <summary>
            /// A filled order
            /// </summary>
            Filled,

            /// <summary>
            /// A cancelled order
            /// </summary>
            Canceled,

            /// <summary>
            /// An order pending for cancelling
            /// </summary>
            PendingCancel,

            /// <summary>
            /// A rejected order
            /// </summary>
            Rejected,

            /// <summary>
            /// An expired order
            /// </summary>
            Expired,
        }

        /// <summary>
        /// Gets the unique id of the order
        /// </summary>
        public long OrderId { get; }

        /// <summary>
        /// Get the price at which the order was set.
        /// </summary>
        public decimal SetPrice { get; }

        /// <summary>
        /// Gets or sets the average price of the order.
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Gets or sets the last fill price of the order.
        /// </summary>
        public decimal LastFillPrice { get; set; }

        /// <summary>
        /// Gets the side of the order.
        /// </summary>
        public OrderSide Side { get; }

        /// <summary>
        /// Gets or sets the status of the order
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Gets the trading pair of the order
        /// </summary>
        public TradingPair Pair { get; }

        /// <summary>
        /// Gets the total amount of the order
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Gets or sets the filled amount of the order.
        /// </summary>
        public decimal TotalFilled { get; set; }

        /// <summary>
        /// Gets or sets the last filled portion of the order.
        /// </summary>
        public decimal LastFillIncrement { get; set; }
    }
}
