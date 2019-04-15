using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Tests.Stubs;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Singleton for ServiceProvider.
    /// </summary>
    internal sealed class ServiceProviderSingleton
    {
        private static readonly Lazy<ServiceProviderSingleton> Lazy =
            new Lazy<ServiceProviderSingleton>(() => new ServiceProviderSingleton());

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderSingleton"/> class.
        /// </summary>
        private ServiceProviderSingleton()
        {
            // Inject commandline args
            var args = new CommandLineArgs { SkipDatabase = true };
            typeof(Program).GetProperty("CommandLineArgs").SetValue(null, args);

            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide dependencies for services
            Startup startup = new Startup("appsettings.yaml");
            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            services.Replace(new ServiceDescriptor(
                typeof(IPortfolioFetcherService),
                typeof(TestPortfolioFetcher),
                ServiceLifetime.Transient));

            services.Replace(new ServiceDescriptor(
                typeof(AllocationManager),
                typeof(TestAllocationManager),
                ServiceLifetime.Singleton));

            // Create service provider
            ServiceProvider = services.BuildServiceProvider();

            // Configure application
            Startup.Configure(ServiceProvider, (ILoggerFactory)ServiceProvider.GetService(typeof(ILoggerFactory)));
        }

        /// <summary>
        /// Gets the instance of a ServiceProviderSingleton.
        /// </summary>
        public static ServiceProviderSingleton Instance => Lazy.Value;

        /// <summary>
        /// Gets the ServiceProvider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }
    }
}
