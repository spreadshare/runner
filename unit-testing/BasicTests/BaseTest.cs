using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public abstract class BaseTest
    {
        protected ILogger Logger;

        protected BaseTest()
        {
            var loggerFactory = (ILoggerFactory) ServiceProviderSingleton.Instance
                .ServiceProvider.GetService(typeof(ILoggerFactory));
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
