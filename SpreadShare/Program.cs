using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.Strategy;
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
            // Start service to fetch exchange data
            var service = serviceProvider.GetService<IFetchCandles>();
            service.Connect();

            var trading = serviceProvider.GetService<ITradingService>();
            trading.Start();

            var user = serviceProvider.GetService<IUserService>();

            var result = user.Start();

            // Start strategy service
            if (result.Code == ResponseCodes.Success)
            {
                var strategy = serviceProvider.GetService<IStrategy>();
                strategy.Start();
            }
            
            // Start zeroMQ service
            var zeroMqService = serviceProvider.GetService<IZeroMqService>();
            zeroMqService.Start();

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
