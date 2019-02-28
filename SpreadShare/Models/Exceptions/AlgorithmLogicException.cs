using System;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception thrown when something is wrong with the Algorithm Logic.
    /// </summary>
    public class AlgorithmLogicException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmLogicException"/> class.
        /// </summary>
        public AlgorithmLogicException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmLogicException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public AlgorithmLogicException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmLogicException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public AlgorithmLogicException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}