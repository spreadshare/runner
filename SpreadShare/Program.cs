using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.ZeroMQ;

namespace SpreadShare
{
    /// <summary>
    /// Entrypoint of the application
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entrypoint of the application
        /// </summary>
        public static void Main()
        {
            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            ILoggerFactory loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            Startup.Configure(serviceProvider, loggerFactory);

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            ExecuteBusinessLogic(serviceProvider, loggerFactory);

            KeepRunningForever();
        }

        /// <summary>
        /// Start business services
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        private static void ExecuteBusinessLogic(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger("ExecuteBusinessLogic");
            SettingsService settings = (SettingsService)serviceProvider.GetService<ISettingsService>();

            // Start the exchange factory, which also starts any communications with exchanges.
            var factory = serviceProvider.GetService<ExchangeFactoryService>();
            var factoryResult = factory.Start();
            if (!factoryResult.Success)
            {
                logger.LogError($"Exchange Factory Service failed to start! {factoryResult}");
            }

            // Start algorithm service
            if (!factoryResult.Success)
            {
                logger.LogCritical("Exchange Factory service is not running, the algorithms can not start.");
                throw new ArgumentException("Algorithm Service depends on TradingService, which is disabled");
            }

            var algorithm = serviceProvider.GetService<IAlgorithmService>();
            foreach (var name in settings.EnabledServices.Algorithms.Keys)
            {
                if (settings.EnabledServices.Algorithms[name])
                {
                    var algorithmResponse = algorithm.StartAlgorithm(typeof(SimpleBandWagonAlgorithm));
                    if (algorithmResponse.Code != ResponseCode.Success)
                    {
                        logger.LogError($"algorithm failed to start, report: {algorithmResponse}");
                    }
                }
            }

            // Start ZeroMQ command listener and broadcaster
            if (settings.EnabledServices.ZeroMq)
            {
                var zeroMq = serviceProvider.GetService<IZeroMqService>();
                var zeroMqResult = zeroMq.Start();
                if (zeroMqResult.Success)
                {
                    logger.LogInformation("ZeroMqService has started");
                }
                else
                {
                    logger.LogError($"ZeroMqService could not be started: {zeroMqResult.Message}");
                }
            }
            else
            {
                logger.LogInformation("ZeroMqService has been disabled. If you want to enable the ZeroMqService," +
                                      "you must change this in appsettings.json");
            }
        }

        /// <summary>
        /// Keep the application running
        /// </summary>
        private static void KeepRunningForever()
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
