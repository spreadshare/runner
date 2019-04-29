using System.ComponentModel.DataAnnotations.Schema;

namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Object corresponding with entries in the 'Table' View.
    /// </summary>
    [Table("Trades")]
    internal class Trade
    {
        /// <summary>
        /// Gets or sets the ID of the trade (only unique within a session).
        /// </summary>
        public int TradeId { get; set; }

        /// <summary>
        /// Gets or sets the performance of the trade. Read as the end value over the begin value.
        /// </summary>
        public decimal Performance { get; set; }

        /// <summary>
        /// Gets or sets the total bought volume of the asset, in non base currency.
        /// </summary>
        public decimal BuyVolume { get; set; }

        /// <summary>
        /// Gets or sets the total bought volume of the asset, in base currency.
        /// </summary>
        public decimal BuyVolumeQuote { get; set; }

        /// <summary>
        /// Gets or sets the total sold volume of the asset, in non base currency.
        /// </summary>
        public decimal SellVolume { get; set; }

        /// <summary>
        /// Gets or sets total sold volume of the asset, in base currency.
        /// </summary>
        public decimal SellVolumeQuote { get; set; }

        /// <summary>
        /// Gets or sets the average buy price.
        /// </summary>
        public decimal AverageBuyPrice { get; set; }

        /// <summary>
        /// Gets or sets the average sell price.
        /// </summary>
        public decimal AverageSellPrice { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds between the first buy order and the last sell order.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the trade.
        /// </summary>
        public string Pair { get; set; }

        /// <summary>
        /// Gets or sets the total buy commission of the trade.
        /// </summary>
        public decimal BuyCommission { get; set; }

        /// <summary>
        /// Gets or sets the asset with which the buy commission was paid.
        /// </summary>
        public string BuyCommissionAsset { get; set; }

        /// <summary>
        /// Gets or sets the total sell commission of the trade.
        /// </summary>
        public decimal SellCommission { get; set; }

        /// <summary>
        /// Gets or sets the asset with which the sell commission was paid.
        /// </summary>
        public string SellCommissionAsset { get; set; }

        /// <summary>
        /// Gets or sets the Session associated with this trade.
        /// </summary>
        public AlgorithmSession Session { get; set; }
    }
}