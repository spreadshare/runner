using System;
using System.Globalization;
using System.IO;
using System.Text;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Backtesting
{
    /// <summary>
    /// File writing agent for producing backtest reports.
    /// </summary>
    internal class BacktestOutputAgent
    {
        private StringBuilder _outputStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutputAgent"/> class.
        /// </summary>
        public BacktestOutputAgent()
        {
            _outputStream = new StringBuilder();
        }

        /// <summary>
        /// Register a trade for the backtest log
        /// </summary>
        /// <param name="timestamp">time of the trade</param>
        /// <param name="baseAsset">base asset</param>
        /// <param name="counterAsset">non-base asset</param>
        /// <param name="side">buy or sell</param>
        /// <param name="quantity">amount of non-base asset</param>
        /// <param name="feeAsset">asset used to pay the fees</param>
        /// <param name="fees">amount of fees</param>
        public void RegisterTradeEvent(
            DateTime timestamp,
            Currency baseAsset,
            Currency counterAsset,
            OrderSide side,
            decimal quantity,
            Currency feeAsset,
            decimal fees)
        {
            string[] items =
            {
                timestamp.ToUniversalTime().ToString(CultureInfo.InvariantCulture),
                baseAsset.ToString(),
                counterAsset.ToString(),
                side.ToString(),
                quantity.ToString(CultureInfo.InvariantCulture),
                feeAsset.ToString() + fees.ToString(CultureInfo.InvariantCulture)
            };

            _outputStream.Append(string.Join(",", items));
        }

        /// <summary>
        /// Flush collected lines to file
        /// </summary>
        /// <param name="filepath">The filename to save it to</param>
        public void FlushToDisk(string filepath)
        {
            File.WriteAllText(filepath, _outputStream.ToString());
            _outputStream = new StringBuilder();
        }
    }
}