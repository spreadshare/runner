namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Model containing information about an executed trade.
    /// </summary>
    internal class TradeExecution
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeExecution"/> class.
        /// </summary>
        /// <param name="from">The asset value on the left side of the trade.</param>
        /// <param name="to">The asset value on the right side of the trade.</param>
        public TradeExecution(Balance from, Balance to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the left side of the executed trade.
        /// </summary>
        public Balance From { get; }

        /// <summary>
        /// Gets the right side of the executed trade.
        /// </summary>
        public Balance To { get; }
    }
}