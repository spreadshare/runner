using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Strategy;
using SpreadShare.Strategy.Implementations;
using SpreadShare.SupportServices;
using SpreadShare.ZeroMQ;

namespace SpreadShare
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor: Setting configuration
        /// </summary>
        /// <param name="jsonfile">Filename of json file</param>
        public Startup(string jsonfile = "appsettings.json")
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(jsonfile)
                .Build();
        }

        /// <summary>
        /// Configure support services such as databases and logging
        /// </summary>
        /// <param name="services"></param>
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
        }

        /// <summary>
        /// Configure business logic services such as fetching exchange data
        /// </summary>
        /// <param name="services">Collection of services</param>
        public void ConfigureBusinessServices(IServiceCollection services)
        {
            // Add BinanceFetchCandles dependency
            services.AddSingleton<IFetchCandles, BinanceFetchCandles>();

            // Add Binance Rest API dependency
            services.AddSingleton<ITradingService, BinanceTradingService>();

            // Add Binance User Websocket dependency
            services.AddSingleton<IUserService, BinanceUserService>();

            // Strategy to be executed
            services.AddSingleton<IStrategy, SimpleBandWagonStrategy>();

            // ZeroMQ Service to interface with other programs
            services.AddSingleton<IZeroMqService, ZeroMqService>();
        }

        /// <summary>
        /// Additional configuration after all have been configured
        /// </summary>
        /// <param name="serviceProvider">Provides access to configured services</param>
        public void Configure(IServiceProvider serviceProvider)
        {
            // Migrate the database (https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
            var service = serviceProvider.GetService<IDatabaseMigrationService>();
            service.Migrate();
        }
    }
}
