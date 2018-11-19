using System;
using System.ComponentModel.DataAnnotations;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models
{
    /// <summary>
    /// Models a trade as found in the database
    /// </summary>
    internal class DatabaseTrade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTrade"/> class.
        /// </summary>
        /// <param name="timestamp">The unix timestamp in milliseconds</param>
        /// <param name="pair">The trading pair</param>
        /// <param name="quantity">The amount of non base currency</param>
        /// <param name="price">The price of the trade</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="assets">The portfolio after the trade</param>
        /// <param name="value">The value of the portfolio before the trade</param>
        public DatabaseTrade(
            long timestamp,
            string pair,
            decimal quantity,
            decimal price,
            string side,
            string assets,
            decimal value)
        {
            Timestamp = timestamp;
            Pair = pair;
            Price = price;
            Quantity = quantity;
            Side = side;
            Assets = assets;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the ID of the trade
        /// </summary>
        [Key]
        public long ID { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the trade
        /// </summary>
        public long Timestamp { get; set; }

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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ID}, {Timestamp}, {Pair}, {Quantity}, {Price}, {Side}, {Assets}, {Value}";
        }
    }
}