using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when the backtest is out of data.
    /// </summary>
    public class BacktestOutOfDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutOfDataException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public BacktestOutOfDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutOfDataException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public BacktestOutOfDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutOfDataException"/> class.
        /// </summary>
        public BacktestOutOfDataException()
        {
        }
    }
}