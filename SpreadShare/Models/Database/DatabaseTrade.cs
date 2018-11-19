using System.ComponentModel.DataAnnotations;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Models a trade as found in the database
    /// </summary>
    internal class DatabaseTrade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTrade"/> class.
        /// </summary>
        /// <param name="orderId">The orderId of the trade, must be unique</param>
        /// <param name="orderType">Kind of order</param>
        /// <param name="createdTimestamp">The unix createdTimestamp in milliseconds</param>
        /// <param name="filledTimeStamp">Time at which the order was filled</param>
        /// <param name="pair">The trading pair</param>
        /// <param name="quantity">The amount of non base currency</param>
        /// <param name="price">The price of the trade</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="assets">The portfolio after the trade</param>
        /// <param name="value">The value of the portfolio before the trade</param>
        public DatabaseTrade(
            long orderId,
            string orderType,
            long createdTimestamp,
            long filledTimeStamp,
            string pair,
            decimal quantity,
            decimal price,
            string side,
            string assets,
            decimal value)
        {
            OrderId = orderId;
            OrderType = orderType;
            CreatedTimestamp = createdTimestamp;
            FilledTimeStamp = filledTimeStamp;
            Pair = pair;
            Price = price;
            Quantity = quantity;
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
            CreatedTimestamp = order.CreatedTimeStamp;
            FilledTimeStamp = order.FilledTimeStamp;
            Pair = order.Pair.ToString();
            Price = order.AveragePrice;
            Quantity = order.Amount;
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
        /// Gets or sets the quantity of the trade
        /// </summary>
        public decimal Quantity { get; set; }

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
        /// Get a header matching the format of ToString()
        /// </summary>
        /// <returns>string header</returns>
        public static string GetCsvHeader()
        {
            return $"{nameof(OrderId)}, " +
                   $"{nameof(OrderType)}, " +
                   $"{nameof(Side)}, " +
                   $"{nameof(CreatedTimestamp)}, " +
                   $"{nameof(FilledTimeStamp)}, " +
                   $"{nameof(Pair)}, " +
                   $"{nameof(Quantity)}, " +
                   $"{nameof(Price)}, " +
                   $"{nameof(Value)}, " +
                   $"{nameof(Assets)}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{OrderId}, " +
                   $"{OrderType}, " +
                   $"{Side}, " +
                   $"{CreatedTimestamp}, " +
                   $"{FilledTimeStamp}, " +
                   $"{Pair}, " +
                   $"{Quantity}, " +
                   $"{Price}, " +
                   $"{Value}, " +
                   $"{Assets}";
        }
    }
}