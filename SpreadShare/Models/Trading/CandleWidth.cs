namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// All the possible widths of a candle.
    /// Integer representations are used in the BacktestTimer.
    /// </summary>
    public enum CandleWidth
    {
        /// <summary>
        /// THIS CANDLE IS FOR TESTING ONLY, DO NOT USE IT.
        /// </summary>
        DONOTUSETestEntry = 7,

        /// <summary>
        /// One minute wide candles.
        /// </summary>
        OneMinute = 1,

        /// <summary>
        /// Three minute wide candles.
        /// </summary>
        ThreeMinutes = 3,

        /// <summary>
        /// Five minute wide candles.
        /// </summary>
        FiveMinutes = 5,

        /// <summary>
        /// Fifteen minute wide candles.
        /// </summary>
        FiveteenMinutes = 15,

        /// <summary>
        /// Thirty minute wide candles.
        /// </summary>
        ThirtyMinutes = 30,

        /// <summary>
        /// Fourtyfive minute wide candles.
        /// </summary>
        FourtyFiveMinutes = 45,
    }
}