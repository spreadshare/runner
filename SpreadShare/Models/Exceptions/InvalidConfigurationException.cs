using System;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.Models.Exceptions
{
    /// <summary>
    /// Exception indicating an invalid Configuration file.
    /// </summary>
    public class InvalidConfigurationException : ExitCodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public InvalidConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        public InvalidConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">innerException.</param>
        public InvalidConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public override ExitCode ExitCode => ExitCode.InvalidConfiguration;
    }
}