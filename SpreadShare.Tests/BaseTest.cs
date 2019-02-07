using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Base class for tests.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput.</param>
        protected BaseTest(ITestOutputHelper outputHelper)
        {
            LoggerFactory = (ILoggerFactory)ServiceProviderSingleton.Instance
                .ServiceProvider.GetService(typeof(ILoggerFactory));
            TestLoggingProvider = new TestLoggingProvider(outputHelper);
            LoggerFactory.AddProvider(TestLoggingProvider);
            Logger = LoggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets logging.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the logging factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets logging provider.
        /// </summary>
        protected TestLoggingProvider TestLoggingProvider { get; }

        /// <summary>
        /// Keep the application running.
        /// </summary>
        protected static void KeepRunningForever()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                }

                // ReSharper disable once FunctionNeverReturns
            });
            t.Start();
            t.Join();
        }
    }
}