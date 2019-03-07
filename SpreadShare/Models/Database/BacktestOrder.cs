using System.ComponentModel.DataAnnotations;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a trade as found in the database.
    /// </summary>
    internal class BacktestOrder : ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOrder"/> class.
        /// </summary>
        /// <param name="orderId">The orderId of the trade, must be unique.</param>
        /// <param name="tradeId">The tradeId of the trade.</param>
        /// <param name="orderType">Kind of order.</param>
        /// <param name="orderStatus">The status of the order.</param>
        /// <param name="createdTimestamp">The unix createdTimestamp in milliseconds.</param>
        /// <param name="filledTimestamp">Time at which the order was filled.</param>
        /// <param name="pair">The trading pair.</param>
        /// <param name="setQuantity">The quantity of non base currency for which the order was set.</param>
        /// <param name="filledQuantity">The quantity of non base currency that was filled.</param>
        /// <param name="setPrice">The setPrice of the trade.</param>
        /// <param name="filledPrice">The filledPrice of the trade.</param>
        /// <param name="side">Buy or sell order.</param>
        /// <param name="assets">The portfolio after the trade.</param>
        /// <param name="value">The value of the portfolio before the trade.</param>
        public BacktestOrder(
            long orderId,
            long tradeId,
            string orderType,
            string orderStatus,
            long createdTimestamp,
            long filledTimestamp,
            string pair,
            decimal setQuantity,
            decimal filledQuantity,
            decimal setPrice,
            decimal filledPrice,
            string side,
            string assets,
            decimal value)
        {
            OrderId = orderId;
            TradeId = tradeId;
            OrderType = orderType;
            OrderStatus = orderStatus;
            CreatedTimestamp = createdTimestamp;
            FilledTimestamp = filledTimestamp;
            Pair = pair;
            SetPrice = setPrice;
            FilledPrice = filledPrice;
            SetQuantity = setQuantity;
            FilledQuantity = filledQuantity;
            Side = side;
            Assets = assets;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOrder"/> class.
        /// </summary>
        /// <param name="order">The order containing the information.</param>
        /// <param name="assets">JSON string of the assets after the trade.</param>
        /// <param name="value">Total value of the portfolio after the trade.</param>
        public BacktestOrder(
            OrderUpdate order,
            string assets,
            decimal value)
        {
            OrderType = order.OrderType.ToString();
            TradeId = order.TradeId;
            OrderStatus = order.Status.ToString();
            CreatedTimestamp = order.CreatedTimeStamp;
            FilledTimestamp = order.FilledTimeStamp;
            Pair = order.Pair.ToString();
            SetPrice = order.SetPrice;
            StopPrice = order.StopPrice;
            FilledPrice = order.AverageFilledPrice;

            SetQuantity = order.SetQuantity;
            FilledQuantity = order.FilledQuantity;
            Side = order.Side.ToString();
            Assets = assets;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the ID of the row in the database.
        /// </summary>
        [Key]
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the ID in specific for a certain trade.
        /// </summary>
        public long TradeId { get; set; }

        /// <summary>
        /// Gets or sets the Type of order.
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Gets or sets the Status of the order.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp at the creation of the trade.
        /// </summary>
        public long CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp at the moment the trade was filled.
        /// </summary>
        public long FilledTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the trading pair of the trade.
        /// </summary>
        public string Pair { get; set; }

        /// <summary>
        /// Gets or sets the setQuantity of the trade.
        /// </summary>
        public decimal SetQuantity { get; set; }

        /// <summary>
        /// Gets or sets the filledQuantity of the trade.
        /// </summary>
        public decimal FilledQuantity { get; set; }

        /// <summary>
        /// Gets or sets the setPrice of the trade.
        /// </summary>
        public decimal SetPrice { get; set; }

        /// <summary>
        /// Gets or sets the stopPrice of the trade.
        /// </summary>
        public decimal StopPrice { get; set; }

        /// <summary>
        /// Gets or sets the filledPrice of the trade.
        /// </summary>
        public decimal FilledPrice { get; set; }

        /// <summary>
        /// Gets or sets whether the order was a buy or sell order.
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// Gets or sets the portfolio as it was after the trade.
        /// </summary>
        public string Assets { get; set; }

        /// <summary>
        /// Gets or sets the value of the portfolio in ETH as it was before the trade.
        /// </summary>
        public decimal Value { get; set; }

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
                   $"{nameof(OrderStatus)}{delimiter}" +
                   $"{nameof(Side)}{delimiter}" +
                   $"{nameof(CreatedTimestamp)}{delimiter}" +
                   $"{nameof(FilledTimestamp)}{delimiter}" +
                   $"{nameof(Pair)}{delimiter}" +
                   $"{nameof(SetQuantity)}{delimiter}" +
                   $"{nameof(FilledQuantity)}{delimiter}" +
                   $"{nameof(SetPrice)}{delimiter}" +
                   $"{nameof(StopPrice)}{delimiter}" +
                   $"{nameof(FilledPrice)}{delimiter}" +
                   $"{nameof(Value)}{delimiter}" +
                   $"{nameof(Assets)}";
        }

        /// <inheritdoc />
        public string GetCsvRepresentation(char delimiter)
        {
            return $"{OrderId}{delimiter}" +
                   $"{TradeId}{delimiter}" +
                   $"{OrderType}{delimiter}" +
                   $"{OrderStatus}{delimiter}" +
                   $"{Side}{delimiter}" +
                   $"{CreatedTimestamp}{delimiter}" +
                   $"{FilledTimestamp}{delimiter}" +
                   $"{Pair}{delimiter}" +
                   $"{SetQuantity}{delimiter}" +
                   $"{FilledQuantity}{delimiter}" +
                   $"{SetPrice}{delimiter}" +
                   $"{StopPrice}{delimiter}" +
                   $"{FilledPrice}{delimiter}" +
                   $"{Value}{delimiter}" +
                   $"{Assets}";
        }

        /// <inheritdoc />
        public string GetCsvHeader(char delimiter) => GetStaticCsvHeader(delimiter);
    }
}