using System;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using SpreadShare.Algorithms;
using SpreadShare.Models;
using SpreadShare.SupportServices.BacktestDaemon;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare
{
    /// <summary>
    /// Entrypoint of the application.
    /// </summary>
    internal static class Program
    {
        private static ILoggerFactory _loggerFactory;

        /// <summary>
        /// Gets the instance of the CommandLineArgs.
        /// </summary>
        public static CommandLineArgs CommandLineArgs { get; private set; }

        /// <summary>
        /// Entrypoint of the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Status code.</returns>
        public static int Main(string[] args)
        {
            // Start command line args to local variable.
            Parser.Default.ParseArguments<CommandLineArgs>(args)
                .WithNotParsed(_ => ExitProgramWithCode(ExitCode.InvalidCommandLineArguments))
                .WithParsed(o => CommandLineArgs = o);

            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide dependencies for services
            Startup startup = new Startup(CommandLineArgs.ConfigurationPath);
            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            _loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            Startup.Configure(serviceProvider, _loggerFactory);

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            using (SentrySdk.Init(Configuration.Instance.AdministratorSettings.SentryDSN))
            {
                bool successfulStart = ExecuteBusinessLogic(serviceProvider, _loggerFactory);
                if (!successfulStart)
                {
                    return 1;
                }

                if (CommandLineArgs.Backtesting)
                {
                    var daemonService = serviceProvider.GetService<BacktestDaemonService>();
                    return (int)daemonService.Run();
                }
            }

            return KeepRunningForever();
        }

        /// <summary>
        /// Cause the Main() to return with a given status code.
        /// </summary>
        /// <param name="exitCode">Reason for termination.</param>
        public static void ExitProgramWithCode(ExitCode exitCode)
        {
            // Flush the logs by disposing the factory
            _loggerFactory?.Dispose();
            if (exitCode != ExitCode.Success)
            {
                Console.WriteLine($"Exiting program with code {exitCode}");
            }

            Environment.Exit((int)exitCode);
        }

        /// <summary>
        /// Bind business services.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger.</param>
        /// <returns>Boolean indicating success.</returns>
        private static bool ExecuteBusinessLogic(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger("Program.cs:ExecuteBusinessLogic");
            serviceProvider.GetService<ErrorService>().Bind();
            serviceProvider.GetService<DatabaseUtilities>().Bind();
            serviceProvider.GetService<BacktestDaemonService>().Bind();

            if (CommandLineArgs.Trading)
            {
                // Bind allocated services
                var algorithmService = serviceProvider.GetService<IAlgorithmService>();
                foreach (var algo in Configuration.Instance.EnabledAlgorithms)
                {
                    var algorithmConfigurationType = Reflections.GetMatchingConfigurationsType(algo);
                    var algorithmConfiguration = ConfigurationLoader.LoadConfiguration(algorithmConfigurationType);
                    Console.WriteLine("Loading: " + algorithmConfiguration.GetType());

                    var algorithmResponse = algorithmService.StartAlgorithm(algo, algorithmConfiguration);

                    if (!algorithmResponse.Success)
                    {
                        logger.LogError($"Algorithm failed to start:\n\t {algorithmResponse}");
                        return false;
                    }

                    logger.LogInformation($"Started algorithm '{algo}' successfully");
                }
            }

            return true;
        }

        /// <summary>
        /// Keep the application running.
        /// </summary>
        /// <returns>exit code.</returns>
        private static int KeepRunningForever()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                }
            });
            t.Start();
            t.Join();
            return 0;
        }
    }
}
