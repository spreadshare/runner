using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SpreadShare.Models.Trading;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Class to contains the state of an order update at a certain time.
    /// </summary>
    internal class OrderEvent : IDatabaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEvent"/> class.
        /// </summary>
        /// <param name="session">The session the order belongs to.</param>
        /// <param name="eventTimestamp">The time of the event.</param>
        /// <param name="proxy">The order update model, serving as a proxy.</param>
        public OrderEvent(AlgorithmSession session, long eventTimestamp, OrderUpdate proxy)
        {
            Session = session;
            EventTimestamp = eventTimestamp;
            Proxy = proxy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEvent"/> class. (Used by ef core).
        /// </summary>
        private OrderEvent()
        {
            Proxy = new OrderUpdate();
        }

        #pragma warning disable SA1600
        public bool Finalized => Proxy.Finalized;

        public string CommissionAsset
        {
            get => Proxy.CommissionAsset?.ToString();
            set => Proxy.CommissionAsset = new Currency(value);
        }

        public decimal Commission
        {
            get => Proxy.Commission;
            set => Proxy.Commission = value;
        }

        public decimal LastFillIncrement
        {
            get => Proxy.LastFillIncrement;
            set => Proxy.LastFillIncrement = value;
        }

        public decimal FilledQuantity
        {
            get => Proxy.FilledQuantity;
            set => Proxy.FilledQuantity = value;
        }

        public decimal SetQuantity
        {
            get => Proxy.SetQuantity;
            set => Proxy.SetQuantity = value;
        }

        public string Pair
        {
            get => Proxy.Pair.ToString();
            set => Proxy.Pair = TradingPair.Parse(value);
        }

        public string Status
        {
            get => Proxy.Status.ToString();
            set => Proxy.Status = Enum.Parse<OrderUpdate.OrderStatus>(value);
        }

        public string Side
        {
            get => Proxy.Side.ToString();
            set => Proxy.Side = Enum.Parse<OrderSide>(value);
        }

        public decimal LastFillPrice
        {
            get => Proxy.LastFillPrice;
            set => Proxy.LastFillPrice = value;
        }

        public decimal AverageFilledPrice
        {
            get => Proxy.AverageFilledPrice;
            set => Proxy.AverageFilledPrice = value;
        }

        public decimal StopPrice
        {
            get => Proxy.StopPrice;
            set => Proxy.StopPrice = value;
        }

        public decimal SetPrice
        {
            get => Proxy.SetPrice;
            set => Proxy.SetPrice = value;
        }

        public long FilledTimestamp
        {
            get => Proxy.FilledTimestamp;
            set => Proxy.FilledTimestamp = value;
        }

        public long CreatedTimestamp
        {
            get => Proxy.CreatedTimestamp;
            set => Proxy.CreatedTimestamp = value;
        }

        public string OrderType
        {
            get => Proxy.OrderType.ToString();
            set => Proxy.OrderType = Enum.Parse<OrderUpdate.OrderTypes>(value);
        }

        public long TradeId
        {
            get => Proxy.TradeId;
            set => Proxy.TradeId = value;
        }

        public long OrderId
        {
            get => Proxy.OrderId;
            set => Proxy.OrderId = value;
        }
        #pragma warning restore SA1600

        /// <summary>
        /// Gets or sets a unique identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <inheritdoc />
        public AlgorithmSession Session { get; set; }

        /// <inheritdoc />
        public long EventTimestamp { get; set; }

        [NotMapped]
        private OrderUpdate Proxy { get; set; }
    }
}