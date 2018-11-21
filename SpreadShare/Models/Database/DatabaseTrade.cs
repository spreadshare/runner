using System.ComponentModel.DataAnnotations;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a trade as found in the database
    /// </summary>
    internal class DatabaseTrade : ICsvSerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTrade"/> class.
        /// </summary>
        /// <param name="orderId">The orderId of the trade, must be unique</param>
        /// <param name="orderType">Kind of order</param>
        /// <param name="orderStatus">The status of the order</param>
        /// <param name="createdTimestamp">The unix createdTimestamp in milliseconds</param>
        /// <param name="filledTimeStamp">Time at which the order was filled</param>
        /// <param name="pair">The trading pair</param>
        /// <param name="setQuantity">The quantity of non base currency for which the order was set</param>
        /// <param name="filledQuantity">The quantity of non base currency that was filled</param>
        /// <param name="price">The price of the trade</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="assets">The portfolio after the trade</param>
        /// <param name="value">The value of the portfolio before the trade</param>
        public DatabaseTrade(
            long orderId,
            string orderType,
            string orderStatus,
            long createdTimestamp,
            long filledTimeStamp,
            string pair,
            decimal setQuantity,
            decimal filledQuantity,
            decimal price,
            string side,
            string assets,
            decimal value)
        {
            OrderId = orderId;
            OrderType = orderType;
            OrderStatus = orderStatus;
            CreatedTimestamp = createdTimestamp;
            FilledTimeStamp = filledTimeStamp;
            Pair = pair;
            Price = price;
            SetQuantity = setQuantity;
            FilledQuantity = filledQuantity;
            Side = side;
            Assets = assets;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTrade"/> class.
        /// </summary>
        /// <param name="order">The order containing the information</param>
        /// <param name="assets">JSON string of the assets after the trade</param>
        /// <param name="value">Total value of the portfolio after the trade</param>
        public DatabaseTrade(
            OrderUpdate order,
            string assets,
            decimal value)
        {
            OrderType = order.OrderType.ToString();
            OrderStatus = order.Status.ToString();
            CreatedTimestamp = order.CreatedTimeStamp;
            FilledTimeStamp = order.FilledTimeStamp;
            Pair = order.Pair.ToString();
            Price = order.AveragePrice;
            SetQuantity = order.SetQuantity;
            FilledQuantity = order.FilledQuantity;
            Side = order.Side.ToString();
            Assets = assets;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the ID of the row in the database
        /// </summary>
        [Key]
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Type of order.
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// Gets or sets the Status of the order.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp at the creation of the trade
        /// </summary>
        public long CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Timestamp at the moment the trade was filled
        /// </summary>
        public long FilledTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the trading pair of the trade
        /// </summary>
        public string Pair { get; set; }

        /// <summary>
        /// Gets or sets the setQuantity of the trade
        /// </summary>
        public decimal SetQuantity { get; set; }

        /// <summary>
        /// Gets or sets the filledQuantity of the trade
        /// </summary>
        public decimal FilledQuantity { get; set; }

        /// <summary>
        /// Gets or sets the price of the trade
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets whether the order was a buy or sell order
        /// </summary>
        public string Side { get; set; }

        /// <summary>
        /// Gets or sets the portfolio as it was after the trade
        /// </summary>
        public string Assets { get; set; }

        /// <summary>
        /// Gets or sets the value of the portfolio in ETH as it was before the trade
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Get a header matching the format of the CSV representation
        /// </summary>
        /// <param name="delimiter">delimiter</param>
        /// <returns>csv header</returns>
        public static string GetStaticCsvHeader(char delimiter)
        {
            return $"{nameof(OrderId)}{delimiter} " +
                   $"{nameof(OrderType)}{delimiter} " +
                   $"{nameof(Side)}{delimiter} " +
                   $"{nameof(CreatedTimestamp)}{delimiter} " +
                   $"{nameof(FilledTimeStamp)}{delimiter} " +
                   $"{nameof(Pair)}{delimiter} " +
                   $"{nameof(SetQuantity)}{delimiter} " +
                   $"{nameof(FilledQuantity)}{delimiter} " +
                   $"{nameof(Price)}{delimiter} " +
                   $"{nameof(Value)}{delimiter} " +
                   $"{nameof(Assets)}";
        }

        /// <inheritdoc />
        public string GetCsvRepresentation(char delimiter)
        {
            return $"{OrderId}{delimiter} " +
                   $"{OrderType}{delimiter} " +
                   $"{Side}{delimiter} " +
                   $"{CreatedTimestamp}{delimiter} " +
                   $"{FilledTimeStamp}{delimiter} " +
                   $"{Pair}{delimiter} " +
                   $"{SetQuantity}{delimiter} " +
                   $"{FilledQuantity}{delimiter} " +
                   $"{Price}{delimiter} " +
                   $"{Value}{delimiter} " +
                   $"{Assets}";
        }

        /// <inheritdoc />
        public string GetCsvHeader(char delimiter)
        {
            return GetStaticCsvHeader(delimiter);
        }
    }
}