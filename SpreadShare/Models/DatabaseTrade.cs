using System;
using System.Collections.Generic;
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
        /// <param name="pair">The currency pair</param>
        /// <param name="quantity">The amount of non base currency</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="pre">The portfolio before the trade</param>
        /// <param name="post">The portfolio after the trade</param>
        /// <param name="preValue">The value of the portfolio before the trade</param>
        /// <param name="postValue">The value of the portfolio after the trade</param>
        public DatabaseTrade(
            long timestamp,
            CurrencyPair pair,
            decimal quantity,
            OrderSide side,
            AlgorithmPortfolio pre,
            AlgorithmPortfolio post,
            decimal preValue,
            decimal postValue)
        {
            Timestamp = timestamp;
            Pair = pair;
            Quantity = quantity;
            Side = side;
            Pre = pre;
            Post = post;
            PreValue = preValue;
            PostValue = postValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTrade"/> class.
        /// </summary>
        /// <param name="timestamp">DateTimeOffset object representing the time of the trade</param>
        /// <param name="pair">The currency pair</param>
        /// <param name="quantity">The amount of non base currency</param>
        /// <param name="side">Buy or sell order</param>
        /// <param name="pre">The portfolio before the trade</param>
        /// <param name="post">The portfolio after the trade</param>
        /// <param name="preValue">The value of the portfolio before the trade</param>
        /// <param name="postValue">The value of the portfolio after the trade</param>
        public DatabaseTrade(
            DateTimeOffset timestamp,
            CurrencyPair pair,
            decimal quantity,
            OrderSide side,
            AlgorithmPortfolio pre,
            AlgorithmPortfolio post,
            decimal preValue,
            decimal postValue)
            : this(timestamp.ToUnixTimeMilliseconds(), pair, quantity, side, pre, post, preValue, postValue)
        {
        }

        /// <summary>
        /// Gets or sets the timestamp of the trade
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the currency pair of the trade
        /// </summary>
        public CurrencyPair Pair { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the trade
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets whether the order was a buy or sell order
        /// </summary>
        public OrderSide Side { get; set; }

        /// <summary>
        /// Gets or sets the portfolio as it was before the trade
        /// </summary>
        public AlgorithmPortfolio Pre { get; set; }

        /// <summary>
        /// Gets or sets the portfolio as it was after the trade
        /// </summary>
        public AlgorithmPortfolio Post { get; set; }

        /// <summary>
        /// Gets or sets the value of the portfolio as it was before the trade
        /// </summary>
        public decimal PreValue { get; set; }

        /// <summary>
        /// Gets or sets the value of the portfolio as it was after the trade
        /// </summary>
        public decimal PostValue { get; set; }
    }
}