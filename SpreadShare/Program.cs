using System;
using System.Linq;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare
{
    /// <summary>
    /// Entrypoint of the application
    /// </summary>
    public static class Program
    {
        private static CommandLineArgs _commandLineArgs;

        /// <summary>
        /// Entrypoint of the application
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>Status code</returns>
        public static int Main(string[] args)
        {
            // Bind command line args to local variable.
            Parser.Default.ParseArguments<CommandLineArgs>(args)
                .WithParsed(o => _commandLineArgs = o);

            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup(_commandLineArgs.ConfigurationPath);
            startup.ConfigurationPath = _commandLineArgs.ConfigurationPath;
            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            ILoggerFactory loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            Startup.Configure(serviceProvider, loggerFactory);

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            bool successfulStart = ExecuteBusinessLogic(serviceProvider, loggerFactory);
            if (!successfulStart)
            {
                return 1;
            }

            KeepRunningForever();
            return 0;
        }

        /// <summary>
        /// Start business services
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        /// <returns>Boolean indicating success</returns>
        private static bool ExecuteBusinessLogic(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger("Program.cs:ExecuteBusinessLogic");

            SettingsService settings = serviceProvider.GetService<SettingsService>();

            // Check if allocation either completely set as backtesting, or the --trading flag was used
            if (!_commandLineArgs.Trading)
            {
                decimal sum = 0.0M;

                // Iterate over all exchange types
                foreach (var exchange in settings.AllocationSettings.Keys)
                {
                    // Skip backtesting
                    if (exchange == Exchange.Backtesting)
                    {
                        continue;
                    }

                    // Aggregate all allocation
                    var allocation = settings.AllocationSettings[exchange];
                    sum += allocation.Keys.Select(x => allocation[x]).Aggregate((a, b) => a + b);
                }

                if (sum > 0)
                {
                    logger.LogError("Algorithms where configured with a non-backtesting allocation, " +
                                    "but the --trading flag was off, did you mean to go live?");
                    return false;
                }
            }

            // Start allocated services
            var algorithmService = serviceProvider.GetService<IAlgorithmService>();
            foreach (var algo in settings.EnabledAlgorithms)
            {
                var algorithmResponse = algorithmService.StartAlgorithm(algo);
                if (!algorithmResponse.Success)
                {
                    logger.LogError($"Algorithm failed to start:\n\t {algorithmResponse}");
                    return false;
                }

                logger.LogInformation($"Started algorithm '{algo}' successfully");
            }

            return true;
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
