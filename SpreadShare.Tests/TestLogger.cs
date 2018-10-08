using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Object responsible for logging output to TestOutput
    /// </summary>
    internal class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly List<string> _messages;
        private readonly object _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// </summary>
        /// <param name="outputHelper">Helper that redirects to test output</param>
        /// <param name="messages">Collection containing all messages</param>
        /// <param name="lockObject">Lock object for locking messages</param>
        public TestLogger(ITestOutputHelper outputHelper, ref List<string> messages, ref object lockObject)
        {
            _outputHelper = outputHelper;
            _messages = messages;
            _lock = lockObject;
        }

        /// <summary>
        /// Log a message
        /// </summary>
        /// <typeparam name="TState">Type of message</typeparam>
        /// <param name="logLevel">Log level</param>
        /// <param name="eventId">ID of event</param>
        /// <param name="state">Message</param>
        /// <param name="exception">Exceptions</param>
        /// <param name="formatter">Format of exception</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (_lock)
            {
                _messages.Add(state.ToString());
            }

            try
            {
                _outputHelper.WriteLine(state.ToString());
            }
            catch (AggregateException)
            {
                // Ignored
                // This occurs when another thread is trying to log and the test is already finished
            }
        }

        /// <summary>
        /// Checks whether logging is enabled for given log level
        /// </summary>
        /// <param name="logLevel">Log level to check</param>
        /// <returns>Whether logging is enabled</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return (int)logLevel > 1;
        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <typeparam name="TState">Type of the state</typeparam>
        /// <param name="state">Scope</param>
        /// <returns>Scope disposable</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
