using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Xunit.Abstractions;

namespace Tests
{
    class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly List<string> _messages;

        public TestLogger(ITestOutputHelper outputHelper, ref List<string> messages)
        {
            _outputHelper = outputHelper;
            _messages = messages;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _messages.Add(state.ToString());
            _outputHelper.WriteLine(state.ToString());
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (int)logLevel > 1;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
