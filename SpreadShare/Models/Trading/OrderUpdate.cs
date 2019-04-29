using SpreadShare.Models.Database;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Return value of websockets.
    /// </summary>
    internal class OrderUpdate : ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderUpdate"/> class.
        /// </summary>
        /// <param name="tradeId">The id of the trade.</param>
        /// <param name="orderStatus">The status of the order.</param>
        /// <param name="orderType">The type of the order.</param>
        /// <param name="createdTimestamp">The timestamp at which the order is created.</param>
        /// <param name="setPrice">SetPrice at which the order was set.</param>
        /// <param name="side">Side of the order.</param>
        /// <param name="pair">The pair of order.</param>
        /// <param name="setQuantity">The quantity of the order.</param>
        /// <param name="orderId">The unique id of the order.</param>
        public OrderUpdate(
            long orderId,
            long tradeId,
            OrderStatus orderStatus,
            OrderTypes orderType,
            long createdTimestamp,
            decimal setPrice,
            OrderSide side,
            TradingPair pair,
            decimal setQuantity)
        {
            OrderId = orderId;
            TradeId = tradeId;
            OrderType = orderType;
            CreatedTimestamp = createdTimestamp;
            SetPrice = setPrice;
            Side = side;
            Status = orderStatus;
            Pair = pair;
            SetQuantity = setQuantity;
            SetPrice = setPrice;
            FilledQuantity = 0;
            LastFillIncrement = 0;
            LastFillPrice = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderUpdate"/> class.
        /// </summary>
        public OrderUpdate()
        {
        }

        /// <summary>
        /// The status of an order.
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
        /// Type of the order.
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
            LimitMaker,
        }

        /// <summary>
        /// Gets or sets the unique id of the order.
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the id of the accompanying trade.
        /// </summary>
        public long TradeId { get; set; }

        /// <summary>
        /// Gets or sets the type fo the order.
        /// </summary>
        public OrderTypes OrderType { get; set; }

        /// <summary>
        /// Gets or sets the timestamp at which the order was created.
        /// </summary>
        public long CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp at which the order was filled.
        /// </summary>
        public long FilledTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the price at which the order was set.
        /// </summary>
        public decimal SetPrice { get; set; }

        /// <summary>
        /// Gets or sets the price at which the stoploss order was set.
        /// </summary>
        public decimal StopPrice { get; set; }

        /// <summary>
        /// Gets or sets the average price of the order.
        /// </summary>
        public decimal AverageFilledPrice { get; set; }

        /// <summary>
        /// Gets or sets the last fill price of the order.
        /// </summary>
        public decimal LastFillPrice { get; set; }

        /// <summary>
        /// Gets or sets the side of the order.
        /// </summary>
        public OrderSide Side { get; set;  }

        /// <summary>
        /// Gets or sets the status of the order.
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the trading pair of the order.
        /// </summary>
        public TradingPair Pair { get; set; }

        /// <summary>
        /// Gets or sets the total setQuantity of the order.
        /// </summary>
        public decimal SetQuantity { get; set; }

        /// <summary>
        /// Gets or sets the total filledQuantity of the order.
        /// </summary>
        public decimal FilledQuantity { get; set; }

        /// <summary>
        /// Gets or sets the last filled portion of the order.
        /// </summary>
        public decimal LastFillIncrement { get; set; }

        /// <summary>
        /// Gets or sets the commission amount.
        /// </summary>
        public decimal Commission { get; set; }

        /// <summary>
        /// Gets or sets the commission asset.
        /// </summary>
        public Currency CommissionAsset { get; set; }

        /// <summary>
        /// Gets a value indicating whether the order status will ever change.
        /// </summary>
        public bool Finalized => Status == OrderStatus.Filled || Status == OrderStatus.Rejected
                                 || Status == OrderStatus.Cancelled || Status == OrderStatus.Rejected;

        /// <summary>
        /// Get a header matching the format of the CSV representation.
        /// </summary>
        /// <param name="delimiter">Delimiter.</param>
        /// <returns>csv header.</returns>
        public static string GetStaticCsvHeader(char delimiter)
        {
            return $"{nameof(OrderId)}{delimiter}" +
                   $"{nameof(TradeId)}{delimiter}" +
                   $"{nameof(OrderType)}{delimiter}" +
                   $"{nameof(Status)}{delimiter}" +
                   $"{nameof(Side)}{delimiter}" +
                   $"{nameof(CreatedTimestamp)}{delimiter}" +
                   $"{nameof(FilledTimestamp)}{delimiter}" +
                   $"{nameof(Pair)}{delimiter}" +
                   $"{nameof(SetQuantity)}{delimiter}" +
                   $"{nameof(FilledQuantity)}{delimiter}" +
                   $"{nameof(SetPrice)}{delimiter}" +
                   $"{nameof(StopPrice)}{delimiter}" +
                   $"FilledPrice{delimiter}";
        }

        /// <inheritdoc />
        public string GetCsvRepresentation(char delimiter)
        {
            return $"{OrderId}{delimiter}" +
                   $"{TradeId}{delimiter}" +
                   $"{OrderType}{delimiter}" +
                   $"{Status}{delimiter}" +
                   $"{Side}{delimiter}" +
                   $"{CreatedTimestamp}{delimiter}" +
                   $"{FilledTimestamp}{delimiter}" +
                   $"{Pair}{delimiter}" +
                   $"{SetQuantity}{delimiter}" +
                   $"{FilledQuantity}{delimiter}" +
                   $"{SetPrice}{delimiter}" +
                   $"{StopPrice}{delimiter}" +
                   $"{AverageFilledPrice}{delimiter}";
        }

        /// <inheritdoc />
        public string GetCsvHeader(char delimiter) => GetStaticCsvHeader(delimiter);
    }
}
