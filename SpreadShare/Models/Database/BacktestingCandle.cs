namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Data structure for representing candles.
    /// </summary>
    internal class BacktestingCandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestingCandle"/> class.
        /// </summary>
        /// <param name="openTimestamp">CreatedTimestamp of the candle.</param>
        /// <param name="open">Opening price of candle.</param>
        /// <param name="close">Closing price of candle.</param>
        /// <param name="high">Highest price during time period.</param>
        /// <param name="low">Lowest price during time period.</param>
        /// <param name="volume">Volume in the time period.</param>
        /// <param name="tradingPair">Trading pair of the candle.</param>
        public BacktestingCandle(long openTimestamp, decimal open, decimal close, decimal high, decimal low, decimal volume, string tradingPair)
        {
            OpenTimestamp = openTimestamp;
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;
            TradingPair = tradingPair;
        }

        /// <summary>
        /// Gets or sets the timestamp of the candle.
        /// </summary>
        public long OpenTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the opening price of the candle.
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Gets or sets the closing price of the candle.
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Gets or sets the highest price of the candle.
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Gets or sets the lowest price of the candle.
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Gets or sets the volume of the candle.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the trading pair.
        /// </summary>
        public string TradingPair { get; set; }

        /// <summary>
        /// Gets the average price (not weighted).
        /// </summary>
        public decimal Average => (High + Low) / 2M;
    }
}