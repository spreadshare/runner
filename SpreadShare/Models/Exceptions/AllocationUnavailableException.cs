using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested allocation request cannot be fulfilled.
    /// </summary>
    public class AllocationUnavailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationUnavailableException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public AllocationUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationUnavailableException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public AllocationUnavailableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationUnavailableException"/> class.
        /// </summary>
        public AllocationUnavailableException()
        {
        }
    }
}