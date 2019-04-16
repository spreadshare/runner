using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Logger that writes all logs to the database.
    /// </summary>
    internal class DatabaseEventLogger : ILogger, IDisposable
    {
        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => DatabaseEventListenerService.Log(logLevel, state, exception, formatter);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
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
        protected virtual void Dispose(bool disposing) => Expression.Empty();
    }
}