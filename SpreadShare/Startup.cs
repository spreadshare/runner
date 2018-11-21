using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare
{
    /// <summary>
    /// Startup object for assigning and configuring all services
    /// </summary>
    internal class Startup : IDesignTimeDbContextFactory<DatabaseContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Sets configuration
        /// </summary>
        /// <param name="filepath">location of the configuration file</param>
        public Startup(string filepath)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(filepath)
                .Build();
        }

        /// <summary>
        /// Gets the configuration of the application
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure business logic services such as fetching exchange data
        /// </summary>
        /// <param name="services">Collection of services</param>
        public static void ConfigureBusinessServices(IServiceCollection services)
        {
            // Exchange Factory dappsettingsependency
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
        /// Additional configuration after all have been configured
        /// </summary>
        /// <param name="serviceProvider">Provides access to configured services</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public static void Configure(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger("ConfigureServices");

            // Setup Settings service
            var settings = serviceProvider.GetService<SettingsService>();
            var settingsResult = settings.Start();
            if (!settingsResult.Success)
            {
                logger.LogError($"SettingsService failed to start, aborting other services\n{settingsResult}" +
                                $"Validate that SpreadShare/appsettings.json is in the correct format.");
            }

            // Migrate the database (https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
            var service = serviceProvider.GetService<IDatabaseMigrationService>();
            if (!service.Migrate().Success)
            {
                logger.LogError("Could not migrate database");
            }
        }

        /// <summary>
        /// Configure support services such as databases and logging
        /// </summary>
        /// <param name="services">Collection of services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Database context dependency
            services.AddEntityFrameworkNpgsql().AddDbContext<DatabaseContext>(opt
                => opt.UseNpgsql(Configuration.GetConnectionString("LocalConnection")));

            // TODO: Add layered timeout for unsuccesfully connecting to DB

            // Add Logging dependency
            services.AddLogging(loggingBuilder => loggingBuilder
                .AddConsole(opt => opt.DisableColors = false)
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .SetMinimumLevel(LogLevel.Information));

            // Add Configuration dependency (provides access to appsettings.json)
            services.AddSingleton(Configuration);

            // Add MyService dependency
            services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();

            // Configuration files globals
            services.AddSingleton<SettingsService, SettingsService>();

            // Add Portfolio fetching
            services.AddSingleton<IPortfolioFetcherService, PortfolioFetcherService>();
        }

        /// <summary>
        /// Creates database context
        /// </summary>
        /// <param name="args">Arguments for creating database context</param>
        /// <returns>DatabaseContext</returns>
        public DatabaseContext CreateDbContext(string[] args)
        {
            // Add Database context dependency
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseNpgsql(Configuration.GetConnectionString("LocalConnection"));
            return new DatabaseContext(builder.Options);
        }
    }
}
