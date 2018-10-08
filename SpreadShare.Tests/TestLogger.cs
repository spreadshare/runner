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

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// </summary>
        /// <param name="outputHelper">Helper that redirects to test output</param>
        /// <param name="messages">Collection containing all messages</param>
        public TestLogger(ITestOutputHelper outputHelper, ref List<string> messages)
        {
            _outputHelper = outputHelper;
            _messages = messages;
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
            _messages.Add(state.ToString());
            _outputHelper.WriteLine(state.ToString());
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
