using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Tests
{
    /// <summary>
    /// Singleton for ServiceProvider
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
            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup("appsettings.json");
            startup.ConfigureServices(services);
            Startup.ConfigureBusinessServices(services);

            // Create service provider
            ServiceProvider = services.BuildServiceProvider();

            // Configure application
            Startup.Configure(ServiceProvider, (ILoggerFactory)ServiceProvider.GetService(typeof(ILoggerFactory)));
        }

        /// <summary>
        /// Gets the instance of a ServiceProviderSingleton
        /// </summary>
        public static ServiceProviderSingleton Instance => Lazy.Value;

        /// <summary>
        /// Gets the ServiceProvider
        /// </summary>
        public IServiceProvider ServiceProvider { get; }
    }
}
