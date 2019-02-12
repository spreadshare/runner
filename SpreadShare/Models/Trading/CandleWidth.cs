namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// All the possible widths of a candle.
    /// Integer representations are used in the BacktestTimer.
    /// </summary>
    public enum CandleWidth
    {
        /// <summary>
        /// One minute wide candles.
        /// </summary>
        OneMinute = 1,

        /// <summary>
        /// Five minute wide candles.
        /// </summary>
        FiveMinutes = 5,
    }
}