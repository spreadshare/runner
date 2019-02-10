using System;
using Microsoft.Extensions.Logging;

namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Provides a SentryLogger to any <see cref="LoggerFactory"/> that pushes events with log level ERROR.
    /// <para>
    /// The standard library <see href="https://docs.sentry.io/platforms/dotnet/microsoft-extensions-logging/"/> could
    /// not be used as this project used Yaml configuration instead of Json configuration.
    /// </para>
    /// </summary>
    public class SentryLoggerProvider : ILoggerProvider
    {
        private readonly SentryLogger _sentryLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentryLoggerProvider"/> class.
        /// </summary>
        public SentryLoggerProvider()
        {
            _sentryLogger = new SentryLogger();
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return _sentryLogger;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Mandatory dispose pattern by CA1063.
        /// </summary>
        /// <param name="disposing">Whether to dispose unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sentryLogger?.Dispose();
            }
        }
    }
}