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
        /// <param name="orderType">The type of the order</param>
        /// <param name="createdTimeStamp">The timestamp at which the order is created</param>
        /// <param name="setPrice">Price at which the order was set</param>
        /// <param name="side">Side of the order</param>
        /// <param name="pair">The pair of order</param>
        /// <param name="setQuantity">The quantity of the order</param>
        /// <param name="orderId">The unique id of the order</param>
        public OrderUpdate(
            long orderId,
            OrderTypes orderType,
            long createdTimeStamp,
            decimal setPrice,
            OrderSide side,
            TradingPair pair,
            decimal setQuantity)
        {
            OrderId = orderId;
            OrderType = orderType;
            CreatedTimeStamp = createdTimeStamp;
            SetPrice = setPrice;
            Side = side;
            Status = OrderStatus.New;
            Pair = pair;
            SetQuantity = setQuantity;
            SetPrice = setPrice;
            FilledQuantity = 0;
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
            Cancelled,

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
        /// Type of the order
        /// </summary>
        public enum OrderTypes
        {
            /// <summary>
            /// A market order
            /// </summary>
            Market,

            /// <summary>
            /// A limit order
            /// </summary>
            Limit,

            /// <summary>
            /// A stoploss order
            /// </summary>
            StopLoss,

            /// <summary>
            /// A stoploss limit order
            /// </summary>
            StopLossLimit,

            /// <summary>
            /// A take profit order
            /// </summary>
            TakeProfit,

            /// <summary>
            /// A take profit limit order
            /// </summary>
            TakeProfitLimit,

            /// <summary>
            /// A limit maker order
            /// </summary>
            LimitMaker
        }

        /// <summary>
        /// Gets the unique id of the order
        /// </summary>
        public long OrderId { get; }

        /// <summary>
        /// Gets the type fo the order
        /// </summary>
        public OrderTypes OrderType { get; }

        /// <summary>
        /// Gets the timestamp at which the order was created
        /// </summary>
        public long CreatedTimeStamp { get; }

        /// <summary>
        /// Gets or sets the timestamp at which the order was filled
        /// </summary>
        public long FilledTimeStamp { get; set; }

        /// <summary>
        /// Gets the price at which the order was set.
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
        /// Gets the total setQuantity of the order
        /// </summary>
        public decimal SetQuantity { get; }

        /// <summary>
        /// Gets or sets the total filledAmount of the order
        /// </summary>
        public decimal FilledQuantity { get; set; }

        /// <summary>
        /// Gets or sets the last filled portion of the order.
        /// </summary>
        public decimal LastFillIncrement { get; set; }
    }
}
