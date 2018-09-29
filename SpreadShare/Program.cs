using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.Strategy;

namespace SpreadShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            ILoggerFactory loggerFactory = (ILoggerFactory) serviceProvider.GetService(typeof(ILoggerFactory));
            startup.Configure(serviceProvider, loggerFactory);

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            ExecuteBusinessLogic(serviceProvider, loggerFactory);

            KeepRunningForever();
        }

        private static void ExecuteBusinessLogic(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger("ExecuteBusinessLogic");
            
            var trading = serviceProvider.GetService<ITradingService>();
            var tradingResult = trading.Start();

            var user = serviceProvider.GetService<IUserService>();

            var userResult = user.Start();

            // Start strategy service
            if (userResult.Success && tradingResult.Success)
            {
                var strategy = serviceProvider.GetService<IStrategy>();
                var strategyResult = strategy.Start();
                if (strategyResult.Code != ResponseCodes.Success) {
                    logger.LogError($"Strategy failed to start, report: {strategyResult}");
                }
            } else {
                logger.LogError("Strategy not started because not all needed service started");
                logger.LogError($"User service report: {userResult}");
                logger.LogError($"Trading Service report: {tradingResult}");
            }
        }

        private static void KeepRunningForever()
        {
            Thread t = new Thread(() => { while (true) { Thread.Sleep(1000); } });
            t.Start();
            t.Join();
        }
    }
}
