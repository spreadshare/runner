using System;
using System.Threading;
using CommandLine;
using CSharpx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.ZeroMQ;

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
        public static void Main(string[] args)
        {
            // Bind command line args to local variable.
            Parser.Default.ParseArguments<CommandLineArgs>(args)
                .WithParsed(o => _commandLineArgs = o);

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
            ILogger logger = loggerFactory.CreateLogger("Program.cs:ExecuteBusinessLogic");

            // Read settings from appsettings.json
            SettingsService settings = (SettingsService)serviceProvider.GetService<ISettingsService>();
            var settingsReponse = settings.Start();
            if (!settingsReponse.Success)
            {
                logger.LogError("The program will exit as SettingsService could not be started properly. " +
                                "Please check your configuration in SpreadShare/appsettings.json");
                return;
            }

            // TODO: Check if allocation either completely set as backtesting, or the _commandLineArgs.Trading is enabled.

            // Start allocated services
            var algorithmService = serviceProvider.GetService<IAlgorithmService>();
            foreach (var algo in settings.EnabledAlgorithms)
            {
                var algorithmResponse = algorithmService.StartAlgorithm(algo);
                if (!algorithmResponse.Success)
                {
                    logger.LogError($"Algorithm failed to start:\n\t {algorithmResponse}");
                }

                logger.LogInformation($"Started algorithm '{algo}' successfully");
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
