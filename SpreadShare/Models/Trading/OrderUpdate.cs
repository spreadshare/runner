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
        /// <param name="price">Price of the order update</param>
        /// <param name="side">Side of the order</param>
        /// <param name="status">Status of the order</param>
        /// <param name="pair">The pair of order</param>
        public OrderUpdate(decimal price, OrderSide side, OrderStatus status, TradingPair pair)
        {
            Price = price;
            Side = side;
            Status = status;
            Pair = pair;
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
        /// Gets the price of the order.
        /// </summary>
        public decimal Price { get; set; }

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
    }
}
