using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.BacktestDaemon;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;
using YamlDotNet.Serialization;

namespace SpreadShare
{
    /// <summary>
    /// Startup object for assigning and configuring all services.
    /// </summary>
    internal class Startup : IDesignTimeDbContextFactory<DatabaseContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Empty constructor, used by EF core cli tools.
        /// </summary>
        public Startup()
            : this("appsettings.yaml")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Sets configuration.
        /// </summary>
        /// <param name="filepath">Location of the configuration file.</param>
        public Startup(string filepath)
        {
            // Download all currencies from Binance
            TradingPair.Sync();
            using (var file = new StreamReader(filepath))
            {
                var configuration = new DeserializerBuilder()
                    .Build()
                    .Deserialize<Configuration>(file);
                configuration.Bind();
                ConfigurationValidator.ValidateConstraintsRecursively(configuration);
            }
        }

        /// <summary>
        /// Configure business logic services such as fetching exchange data.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void ConfigureBusinessServices(IServiceCollection services)
        {
            // Exchange Factory dependency
            services.AddSingleton<ExchangeFactoryService, ExchangeFactoryService>();

            // Binance communication dependency
            services.AddSingleton<BinanceCommunicationsService, BinanceCommunicationsService>();

            // Create algorithm service that manages running algorithms
            services.AddSingleton<AlgorithmService, AlgorithmService>();

            // Add allocation manager
            services.AddSingleton<AllocationManager, AllocationManager>();
        }

        /// <summary>
        /// Additional configuration after all have been configured.
        /// </summary>
        /// <param name="serviceProvider">Provides access to configured services.</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger.</param>
        public static void Configure(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("Startup::Configure");

            // Add Sentry to ErrorLogging
            if (Program.CommandLineArgs.Trading)
            {
                loggerFactory.AddProvider(new SentryLoggerProvider());
            }

            // Add DatabaseEventListener to log pipeline.
            loggerFactory.AddProvider(new DatabaseEventLoggerProvider());

            // Migrate the database (https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
            var service = serviceProvider.GetService<DatabaseMigrationService>();
            if (!service.Migrate().Success)
            {
                logger.LogWarning("Could not migrate database.");
                if (Program.CommandLineArgs.Migrate)
                {
                    Program.ExitProgramWithCode(ExitCode.MigrationFailure);
                }
            }
            else
            {
                if (Program.CommandLineArgs.Trading)
                {
                    // Setup event capture services
                    serviceProvider.GetService<DatabaseEventListenerService>().Bind();
                }
                else if (Program.CommandLineArgs.Migrate)
                {
                    Program.ExitProgramWithCode(ExitCode.Success);
                }
            }
        }

        // CA1822: method can be static (method could be static but breaks the logical flow of DI.)
        #pragma warning disable CA1822
        /// <summary>
        /// Configure support services such as databases and logging.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Database context dependency
            services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(
                opt => opt.UseNpgsql(Configuration.Instance.ConnectionStrings.LocalConnection),
                ServiceLifetime.Transient);

            // Add Logging dependency
            services.AddLogging(loggingBuilder => loggingBuilder
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddFilter("SpreadShare", Program.CommandLineArgs.VerboseLogging ? LogLevel.Debug : LogLevel.Critical)
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information));

            services.AddSingleton<DatabaseMigrationService, DatabaseMigrationService>();
            services.AddSingleton<DatabaseUtilities, DatabaseUtilities>();
            services.AddSingleton<DatabaseEventListenerService, DatabaseEventListenerService>();
            services.AddSingleton<BacktestDaemonService, BacktestDaemonService>();

            // Add Portfolio fetching
            switch (Configuration.Instance.EnabledAlgorithm.Exchange)
            {
                case Exchange.Backtesting:
                    services.AddSingleton<IPortfolioFetcherService, BacktestPortfolioFetcher>();
                    break;
                case Exchange.Binance:
                    services.AddSingleton<IPortfolioFetcherService, BinancePortfolioFetcher>();
                    break;
                default:
                    throw new NotImplementedException($"The portfolio fetcher for {Configuration.Instance.EnabledAlgorithm.Exchange} is not linked.");
            }
        }
        #pragma warning restore CA1822

        /// <summary>
        /// Creates database context.
        /// </summary>
        /// <param name="args">Arguments for creating database context.</param>
        /// <returns>DatabaseContext.</returns>
        public DatabaseContext CreateDbContext(string[] args)
        {
            // Add Database context dependency
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseNpgsql(Configuration.Instance.ConnectionStrings.LocalConnection);
            return new DatabaseContext(builder.Options);
        }
    }
}
