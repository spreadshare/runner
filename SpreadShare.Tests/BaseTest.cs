using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.SupportServices.SettingsServices;
using Xunit.Abstractions;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Base class for tests
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// One time iniatilized instance of the settings service.
        /// </summary>
        internal static readonly SettingsService SettingsService =
            (SettingsService)ServiceProviderSingleton.Instance
            .ServiceProvider.GetService<ISettingsService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        protected BaseTest(ITestOutputHelper outputHelper)
        {
            var loggerFactory = (ILoggerFactory)ServiceProviderSingleton.Instance
                .ServiceProvider.GetService(typeof(ILoggerFactory));
            TestLoggingProvider = new TestLoggingProvider(outputHelper);
            loggerFactory.AddProvider(TestLoggingProvider);
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets logging provider
        /// </summary>
        protected TestLoggingProvider TestLoggingProvider { get; }

        /// <summary>
        /// Keep the application running
        /// </summary>
        protected static void KeepRunningForever()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                }
            });
            t.Start();
            t.Join();
        }
    }
}