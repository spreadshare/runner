using System;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Generic model representing a trade.
    /// </summary>
    internal class TradeProposal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeProposal"/> class.
        /// </summary>
        /// <param name="from">The asset value on the left side of the trade</param>
        /// <param name="algorithm">The type of the algorithm that proposes</param>
        public TradeProposal(AssetValue from, Type algorithm)
        {
            From = from;
            Algorithm = algorithm;
        }

        /// <summary>
        /// Gets the left side of the proposed trade
        /// </summary>
        public AssetValue From { get; }

        /// <summary>
        /// Gets the type of the algorithm that proposed the trade
        /// </summary>
        public Type Algorithm { get; }
    }
}