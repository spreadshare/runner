using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Tests
{
    public class TestLoggingProvider : ILoggerProvider
    {
        public List<string> Messages;
        private readonly ITestOutputHelper _outputHelper;

        public TestLoggingProvider(ITestOutputHelper outputHelper)
        {
            Messages = new List<string>();
            _outputHelper = outputHelper;
        }

        public void Dispose()
        {
            Messages = null;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_outputHelper, ref Messages);
        }
    }
}
