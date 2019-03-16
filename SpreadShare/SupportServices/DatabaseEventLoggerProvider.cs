using System;
using Microsoft.Extensions.Logging;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Provider for the logger that writes all logs to the database.
    /// </summary>
    internal class DatabaseEventLoggerProvider : ILoggerProvider
    {
        private readonly DatabaseEventLogger _databaseLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventLoggerProvider"/> class.
        /// </summary>
        public DatabaseEventLoggerProvider()
        {
            _databaseLogger = new DatabaseEventLogger();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
            => _databaseLogger;

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
                _databaseLogger?.Dispose();
            }
        }
    }
}