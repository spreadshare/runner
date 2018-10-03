using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Tests
{
    public class TestLoggingProvider : ILoggerProvider
    {
        private List<string> _messages;
        private readonly ITestOutputHelper _outputHelper;

        public TestLoggingProvider(ITestOutputHelper outputHelper)
        {
            _messages = new List<string>();
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            _messages = null;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_outputHelper, ref _messages);
        }

        public bool ContainsMessage(string target)
        {
            return _messages.Any(message => message.Contains(target));
        }
    }
}
