using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Tests
{
    public abstract class BaseTest
    {
        protected ILogger Logger;
        protected TestLoggingProvider TestLoggingProvider;

        protected BaseTest(ITestOutputHelper outputHelper)
        {
            var loggerFactory = (ILoggerFactory) ServiceProviderSingleton.Instance
                .ServiceProvider.GetService(typeof(ILoggerFactory));
            TestLoggingProvider = new TestLoggingProvider(outputHelper);
            loggerFactory.AddProvider(TestLoggingProvider);
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected static void KeepRunningForever()
        {
            Thread t = new Thread(() => { while (true) { Thread.Sleep(1000); } });
            t.Start();
            t.Join();
        }
    }
}
