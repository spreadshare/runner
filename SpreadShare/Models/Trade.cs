namespace SpreadShare.Models
{
    /// <summary>
    /// Generic model representing a trade.
    /// </summary>
    internal class Trade
    {
        public CurrencyPair Pair { get; set; }
        
        public long TimeStamp { get; set; }
        
        public decimal Amount { get; set; }
        
        public AlgorithmPortfolio Pre { get; set; }
        
        public AlgorithmPortfolio Post { get; set; }
    }
}