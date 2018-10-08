using System;
using Binance.Net.Objects;

namespace SpreadShare.Models
{
    /// <summary>
    /// Object representation of a candle
    /// </summary>
    /// TODO: Should we remove this unused class?
    internal class Candle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Candle"/> class.
        /// </summary>
        public Candle()
        {
        }

        /// <summary>
        /// Gets or sets the id of the candle
        /// </summary>
        public int CandleId { get; set; }

        /// <summary>
        /// Gets or sets the open time of this candle
        /// </summary>
        public DateTime OpenTime { get; set; }

        /// <summary>
        /// Gets or sets the close time of this candle
        /// </summary>
        public DateTime CloseTime { get; set; }

        /// <summary>
        /// Gets or sets the symbol this candle is for
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the open price of this candle
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Gets or sets the close price of this candle
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Gets or sets the higest price of this candle
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Gets or sets the lowest price of this candle
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Gets or sets the volume traded during this candle
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the amount of trades in this candle
        /// </summary>
        public int TradeCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether boolean indicating whether this candle is closed
        /// </summary>
        public bool Final { get; set; }

        /// <summary>
        /// Gets or sets the quote volume
        /// </summary>
        public decimal QuoteAssetVolume { get; set; }

        /// <summary>
        /// Gets or sets the volume of active buy
        /// </summary>
        public decimal TakerBuyBaseAssetVolume { get; set; }

        /// <summary>
        /// Gets or sets the quote volume of active buy
        /// </summary>
        public decimal TakerBuyQuoteAssetVolume { get; set; }

        /// <summary>
        /// Returns a string representation of the candle
        /// </summary>
        /// <returns>A string representation of the candle</returns>
        public override string ToString()
        {
            return $"OpenTime : {OpenTime}, " +
                   $"CloseTime : {CloseTime}, " +
                   $"Symbol : {Symbol}, " +
                   $"Open : {Open}, " +
                   $"Close : {Close}, " +
                   $"High : {High}, " +
                   $"Low : {Low}, " +
                   $"Volume : {Volume}, " +
                   $"TradeCount : {TradeCount}, " +
                   $"Final : {Final}, " +
                   $"QuoteAssetVolume : {QuoteAssetVolume}, " +
                   $"TakerBuyBaseAssetVolume : {TakerBuyBaseAssetVolume}, " +
                   $"TakerBuyQuoteAssetVolume : {TakerBuyQuoteAssetVolume}";
        }
    }
}
