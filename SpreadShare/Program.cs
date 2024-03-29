using System;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using SpreadShare.Algorithms;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.BacktestDaemon;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare
{
    /// <summary>
    /// Entry point of the application.
    /// </summary>
    internal static class Program
    {
        private static readonly IEnv Env = new RealEnvironment();
        private static ILoggerFactory _loggerFactory;

        /// <summary>
        /// Gets the instance of the CommandLineArgs.
        /// </summary>
        public static CommandLineArgs CommandLineArgs { get; private set; }

        /// <summary>
        /// Entry point of the application.
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
            Startup startup;
            try
            {
                startup = new Startup(CommandLineArgs.ConfigurationPath);
            }
            catch (InvalidConfigurationException e)
            {
                Console.ForegroundColor = ConsoleColor.Red; // Logger.logError is not available yet
                Console.WriteLine("fail: Invalid configuration encountered:");
                Console.ForegroundColor = ConsoleColor.Red; // Logger.logError is not available yet
                Console.WriteLine($" > {e.Message}");
                Console.ResetColor();
                return ExitProgramWithCode(ExitCode.InvalidConfiguration);
            }

            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            _loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            Startup.Configure(serviceProvider, _loggerFactory);

            // Setup finished --> Execute business logic services
            if (CommandLineArgs.Trading)
            {
                SentryLogger.GetDsn(out var dsn);
                using (SentrySdk.Init(dsn))
                {
                    ExecuteTradingLogic(serviceProvider);
                }
            }
            else if (CommandLineArgs.Backtesting)
            {
                ExecuteBacktestingLogic(serviceProvider);
            }

            return KeepRunningForever();
        }

        /// <summary>
        /// Cause the Main() to return with a given status code.
        /// </summary>
        /// <param name="exitCode">Reason for termination.</param>
        /// <returns>Exits before returning the exit code.</returns>
        public static int ExitProgramWithCode(ExitCode exitCode)
        {
            // Flush the logs by disposing the factory
            _loggerFactory?.Dispose();
            if (exitCode != ExitCode.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"----------------------------------");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Exiting program with code {exitCode}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"----------------------------------");
            }

            DatabaseEventListenerService.CloseSession(exitCode);

            Env.ExitEnvironment((int)exitCode);
            return (int)exitCode;
        }

        /// <summary>
        /// Setup and run trading algorithms.
        /// </summary>
        /// <param name="serviceProvider">Provides access to services.</param>
        /// <returns>Exits before returning the exit code.</returns>
        private static int ExecuteTradingLogic(IServiceProvider serviceProvider)
        {
            ILogger logger = _loggerFactory.CreateLogger("Program.cs:ExecuteTradingLogic");

            var algorithmService = serviceProvider.GetService<AlgorithmService>();
            var algorithm = Configuration.Instance.EnabledAlgorithm.Algorithm;

            // Link algorithm in configuration to implementation in C#
            AlgorithmConfiguration algorithmConfiguration;
            try
            {
                algorithmConfiguration = Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration;
            }
            catch (InvalidConfigurationException e)
            {
                logger.LogError("Invalid algorithm configuration encountered:\n  > " +
                                $"{e.Message}");
                return ExitProgramWithCode(ExitCode.InvalidConfiguration);
            }

            logger.LogInformation($"Starting algorithm '{algorithm.Name}'");
            logger.LogInformation("Using configuration: " + algorithmConfiguration.GetType().Name);

            // Start algorithm
            var algorithmResponse = algorithmService.StartAlgorithm(algorithm, algorithmConfiguration);

            if (!algorithmResponse.Success)
            {
                logger.LogError($"Algorithm failed to start:\n\t {algorithmResponse}");
                return ExitProgramWithCode(ExitCode.AlgorithmStartupFailure);
            }

            logger.LogInformation($"Started algorithm '{algorithm.Name}' successfully");
            return 0;
        }

        /// <summary>
        /// Setup backtesting and related database services.
        /// </summary>
        /// <param name="serviceProvider">Provides access to services.</param>
        /// <returns>Exits before returning the exit code.</returns>
        private static int ExecuteBacktestingLogic(IServiceProvider serviceProvider)
        {
            // Init database verification checks (checks per backtesting configuration)
            serviceProvider.GetService<DatabaseUtilities>().Bind();

            // Init backtest execution engine
            serviceProvider.GetService<BacktestDaemonService>().Bind();

            // Accept TTY input
            serviceProvider.GetService<BacktestDaemonService>().Run();
            return 0;
        }

        /// <summary>
        /// Keep the application running.
        /// </summary>
        /// <returns>exit code.</returns>
        private static int KeepRunningForever()
        {
            var t = new Thread(() =>
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