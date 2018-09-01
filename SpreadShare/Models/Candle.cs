using System;
using Binance.Net.Objects;

namespace SpreadShare.Models
{
    public class Candle
    {
        public Candle(DateTime openTime, DateTime closeTime, string symbol, decimal open, decimal close,
            decimal high, decimal low, decimal volume, int tradeCount, bool final, decimal quoteAssetVolume,
            decimal takerBuyBaseAssetVolume, decimal takerBuyQuoteAssetVolume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            Symbol = symbol;
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;
            TradeCount = tradeCount;
            Final = final;
            QuoteAssetVolume = quoteAssetVolume;
            TakerBuyBaseAssetVolume = takerBuyBaseAssetVolume;
            TakerBuyQuoteAssetVolume = takerBuyQuoteAssetVolume;
        }

        public Candle(BinanceStreamKline data)
        {
            OpenTime = data.OpenTime;
            CloseTime = data.CloseTime;
            Symbol = data.Symbol;
            Open = data.Open;
            Close = data.Close;
            High = data.High;
            Low = data.Low;
            Volume = data.Volume;
            TradeCount = data.TradeCount;
            Final = data.Final;
            QuoteAssetVolume = data.QuoteAssetVolume;
            TakerBuyBaseAssetVolume = data.TakerBuyBaseAssetVolume;
            TakerBuyQuoteAssetVolume = data.TakerBuyQuoteAssetVolume;
        }


        public int CandleId { get; set; }

        /// <summary>
        /// The open time of this candlestick
        /// </summary>
        public DateTime OpenTime { get; set; }

        /// <summary>
        /// The close time of this candlestick
        /// </summary>
        public DateTime CloseTime { get; set; }

        /// <summary>
        /// The symbol this candlestick is for
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The open price of this candlestick
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// The close price of this candlestick
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// The higest price of this candlestick
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// The lowest price of this candlestick
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// The volume traded during this candlestick
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The amount of trades in this candlestick
        /// </summary>
        public int TradeCount { get; set; }

        /// <summary>
        /// Boolean indicating whether this candlestick is closed
        /// </summary>
        public bool Final { get; set; }

        /// <summary>
        /// The quote volume
        /// </summary>
        public decimal QuoteAssetVolume { get; set; }

        /// <summary>
        /// The volume of active buy
        /// </summary>
        public decimal TakerBuyBaseAssetVolume { get; set; }

        /// <summary>
        /// The quote volume of active buy
        /// </summary>
        public decimal TakerBuyQuoteAssetVolume { get; set; }

        /// <summary>
        /// String format of Candle
        /// </summary>
        /// <returns></returns>
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
