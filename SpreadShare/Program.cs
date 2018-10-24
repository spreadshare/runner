﻿using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
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

            DatabaseContext dbContext = (DatabaseContext)serviceProvider.GetService<DatabaseContext>();
            var count = dbContext.Candles.Count();
            logger.LogCritical($"#####\n{count}\n###");

            // Start the exchange factory
            var factory = serviceProvider.GetService<ExchangeFactoryService>();
            var factoryResult = factory.Start();
            if (!factoryResult.Success)
            {
                logger.LogError($"Exchange Factory Service failed to start! {factoryResult}");
            }

            // Start StrategyService
            if (settings.EnabledServices.Algorithm)
            {
                if (!settings.EnabledServices.Trading)
                {
                    logger.LogCritical("TradingService is not enabled. You must enable tradingService in " +
                                       "appsettings.json if you want to use the UserService");
                    throw new ArgumentException("StrategyService depends on TradingService, which is disabled");
                }

                if (!settings.EnabledServices.User)
                {
                    logger.LogCritical("UserService is not enabled. You must enable userService in " +
                                       "appsettings.json if you want to use the UserService");
                    throw new ArgumentException("StrategyService depends on TradingService, which is disabled");
                }

                var algorithm = serviceProvider.GetService<IAlgorithmService>();
                var algorithmResponse = algorithm.StartAlgorithm(typeof(SimpleBandWagonAlgorithmSettings));
                if (algorithmResponse.Code != ResponseCode.Success)
                {
                    logger.LogError($"algorithm failed to start, report: {algorithmResponse}");
                }
            }
            else
            {
                logger.LogInformation("StrategyService has been disabled. If you want to enable the StrategyService," +
                                      "you must change this in appsettings.json");
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
