using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Tests
{
    public class TestLoggingProvider : ILoggerProvider
    {
        public List<string> Messages;
        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Constructor: Create LoggerProvider
        /// </summary>
        /// <param name="outputHelper">Helper that redirects output to test output</param>
        public TestLoggingProvider(ITestOutputHelper outputHelper)
        {
            Messages = new List<string>();
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// Method called when LoggerProvider is disposed
        /// </summary>
        public void Dispose()
        {
            Messages = null;
        }

        /// <summary>
        /// Creates logger
        /// </summary>
        /// <param name="categoryName">Name of logger</param>
        /// <returns>ILogger instance</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger(_outputHelper, ref Messages);
        }
    }
}
