using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.Strategy;
using SpreadShare.SupportServices;
using SpreadShare.ZeroMQ;

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
            startup.Configure(serviceProvider, (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory)));

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            ExecuteBusinessLogic(serviceProvider);

            KeepRunningForever();
        }

        private static void ExecuteBusinessLogic(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetService<ISettingsService>();
            var configurationResult = configuration.Start();
            if (!configurationResult.Success) {
                Console.WriteLine("SettingsService failed to start, aborting other services");
                return;
            }

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
                    Console.WriteLine($"Strategy failed to start, report: {strategyResult}");
                }
            } else {
                Console.WriteLine("Strategy not started because not all needed service started");
                Console.WriteLine($"User service report: {userResult}");
                Console.WriteLine($"Trading Service report: {tradingResult}");
            }

            KeepRunningForever();
        }

        private static void KeepRunningForever()
        {
            Thread t = new Thread(() => { while (true) { Thread.Sleep(1000); } });
            t.Start();
            t.Join();
        }
    }
}
