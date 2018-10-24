using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models
{
    /// <summary>
    /// Data structure for representing candles.
    /// </summary>
    internal class DBCandle
    {
        /// <summary>
        /// Gets or sets the Timestamp
        /// </summary>
        [Key]
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the open price
        /// </summary>
        public decimal Open { get; set; }
        
        /// <summary>
        /// Gets or sets the close price
        /// </summary>
        public decimal Close { get; set; }
        
        /// <summary>
        /// Gets or sets the high price
        /// </summary>
        public decimal High { get; set; }
        
        /// <summary>
        /// Gets or sets the low price
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Gets the average price (not weighted)
        /// </summary>
        public decimal Average => (High + Low) / 2M;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCandle"/> class.
        /// </summary>
        /// <param name="timestamp">timestamp</param>
        /// <param name="open">open price</param>
        /// <param name="close">close price</param>
        /// <param name="high">high price</param>
        /// <param name="low">low price</param>
        public DBCandle(long timestamp, decimal open, decimal close, decimal high, decimal low)
        {
            Timestamp = timestamp;
            Open = open;
            Close = close;
            High = high;
            Low = low;
        }
    }
}