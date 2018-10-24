using System.ComponentModel.DataAnnotations;

namespace SpreadShare.Models
{
    public class DBCandle
    {
        [Key]
        public uint Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }

        public DBCandle(uint timestamp, decimal open, decimal close, decimal high, decimal low)
        {
            Timestamp = timestamp;
            Open = open;
            Close = close;
            High = high;
            Low = low;
        }
    }
}