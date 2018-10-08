using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Provides logging to tests
    /// </summary>
    public class TestLoggingProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _outputHelper;
        private List<string> _messages;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLoggingProvider"/> class.
        /// </summary>
        /// <param name="outputHelper">Helper that redirects output to test output</param>
        public TestLoggingProvider(ITestOutputHelper outputHelper)
        {
            _messages = new List<string>();
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// Gets list of output messages
        /// </summary>
        public List<string> Messages => _messages;

        /// <summary>
        /// Creates logger
        /// </summary>
        /// <param name="categoryName">Name of logger</param>
        /// <returns>ILogger instance</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_outputHelper, ref _messages);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object's resource
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messages = null;
            }
        }
    }
}
