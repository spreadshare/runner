using System.ComponentModel.DataAnnotations;
using SpreadShare.Models.Trading;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Class to contains the state of an order update at a certain time.
    /// </summary>
    internal class OrderEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEvent"/> class.
        /// </summary>
        /// <param name="orderId">orderId.</param>
        /// <param name="tradeId">tradeId.</param>
        /// <param name="orderType">orderType.</param>
        /// <param name="orderStatus">orderStatus.</param>
        /// <param name="createdTimestamp">createdTimestamp.</param>
        /// <param name="filledTimestamp">filledTimeStamp.</param>
        /// <param name="pair">pair.</param>
        /// <param name="setQuantity">quantity.</param>
        /// <param name="filledQuantity">filledQuantity.</param>
        /// <param name="setPrice">price.</param>
        /// <param name="stopPrice">stopPrice.</param>
        /// <param name="filledPrice">filledPrice.</param>
        /// <param name="side">side.</param>
        public OrderEvent(
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
            decimal stopPrice,
            decimal filledPrice,
            string side)
        {
            OrderId = orderId;
            TradeId = tradeId;
            OrderType = orderType;
            OrderStatus = orderStatus;
            CreatedTimestamp = createdTimestamp;
            FilledTimestamp = filledTimestamp;
            Pair = pair;
            SetQuantity = setQuantity;
            FilledQuantity = filledQuantity;
            SetPrice = setPrice;
            StopPrice = stopPrice;
            FilledPrice = filledPrice;
            Side = side;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEvent"/> class.
        /// </summary>
        /// <param name="order">The order update to pull data from.</param>
        public OrderEvent(OrderUpdate order)
        {
            OrderId = order.OrderId;
            TradeId = order.TradeId;
            OrderType = order.OrderType.ToString();
            OrderStatus = order.Status.ToString();
            CreatedTimestamp = order.CreatedTimeStamp;
            FilledTimestamp = order.FilledTimeStamp;
            Pair = order.Pair.ToString();
            SetQuantity = order.SetQuantity;
            FilledQuantity = order.FilledQuantity;
            SetPrice = order.SetPrice;
            StopPrice = order.StopPrice;
            FilledPrice = order.AverageFilledPrice;
            Side = order.Side.ToString();
        }

        /// <summary>
        /// Gets or sets the ID of the row in the database.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the order event.
        /// </summary>
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
    }
}