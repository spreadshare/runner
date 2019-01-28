using System;
using System.Diagnostics;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown by ExchangeProviders.
    /// </summary>
    public abstract class ProviderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        public ProviderException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public ProviderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public ProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the stack frame from which this exception was thrown.
        /// </summary>
        public StackFrame StackFrame { get; } = new StackFrame(1, true);
    }
}