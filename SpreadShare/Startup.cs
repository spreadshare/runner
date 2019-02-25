using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
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
        /// Empty constructor, visited by EF core cli tools.
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
            using (var file = new StreamReader(filepath))
            {
                var configuration = new DeserializerBuilder()
                    .Build()
                    .Deserialize<Configuration>(file);
                ConfigurationValidator.ValidateConstraintsRecursively(configuration);
                configuration.Bind();
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

            services.AddSingleton<BacktestCommunicationService, BacktestCommunicationService>();

            // Create algorithm service that manages running algorithms
            services.AddSingleton<IAlgorithmService, AlgorithmService>();

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
            ILogger logger = loggerFactory.CreateLogger("ConfigureServices");

            // Add Sentry to ErrorLogging
            loggerFactory.AddProvider(new SentryLoggerProvider());

            // Download all currencies from Binance
            TradingPair.Sync(logger);

            // Migrate the database (https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
            var service = serviceProvider.GetService<IDatabaseMigrationService>();
            if (!service.Migrate().Success)
            {
                logger.LogWarning("Could not migrate database");
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
            services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(opt
                => opt.UseNpgsql(Configuration.Instance.ConnectionStrings.LocalConnection));

            // TODO: Add layered timeout for unsuccessfully connecting to DB
            // Add Logging dependency
            services.AddLogging(loggingBuilder => loggingBuilder
                .AddConsole(opt => opt.DisableColors = false)
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddFilter("SpreadShare", Program.CommandLineArgs.VerboseLogging ? LogLevel.Debug : LogLevel.Critical)
                .SetMinimumLevel(LogLevel.Information));

            // Add MyService dependency
            services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();

            // Database utilities
            services.AddSingleton<DatabaseUtilities, DatabaseUtilities>();

            // Backtesting service.
            services.AddSingleton<BacktestDaemonService, BacktestDaemonService>();

            // Add Portfolio fetching
            services.AddSingleton<IPortfolioFetcherService, PortfolioFetcherService>();
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
