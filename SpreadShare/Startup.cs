using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Binance;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.ZeroMQ;

namespace SpreadShare
{
    /// <summary>
    /// Startup object for assigning and configuring all services
    /// </summary>
    internal class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// Sets configuration
        /// </summary>
        /// <param name="jsonfile">Filename of json file</param>
        public Startup(string jsonfile = "appsettings.json")
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(jsonfile)
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
            // Add Binance Rest API dependency
            services.AddSingleton<ITradingService, BinanceTradingService>();

            // Add Binance User Websocket dependency
            services.AddSingleton<IUserService, BinanceUserService>();

            // Exchange Factory dependency
            services.AddSingleton<ExchangeFactoryService, ExchangeFactoryService>();

            // Binance Communication dependency
            services.AddSingleton<BinanceCommunicationsService, BinanceCommunicationsService>();

            // Algorithm to be executed
            services.AddSingleton<IAlgorithm, SimpleBandWagonAlgorithm>();

            // ZeroMQ Service to interface with other programs
            services.AddSingleton<IZeroMqService, ZeroMqService>();
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
            var settings = serviceProvider.GetService<ISettingsService>();
            var settingsResult = settings.Start();
            if (!settingsResult.Success)
            {
                logger.LogError($"SettingsService failed to start, aborting other services\n{settingsResult}");
            }

            // Migrate the database (https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
            var service = serviceProvider.GetService<IDatabaseMigrationService>();
            if (service.Migrate().Code == ResponseCode.Success)
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
                .AddConsole(opt => opt.DisableColors = true)
                .SetMinimumLevel(LogLevel.Information));

            // Add Configuration dependency (provides access to appsettings.json)
            services.AddSingleton(Configuration);

            // Add MyService dependency
            services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();

            // Configuration files globals
            services.AddSingleton<ISettingsService, SettingsService>();
        }
    }
}
