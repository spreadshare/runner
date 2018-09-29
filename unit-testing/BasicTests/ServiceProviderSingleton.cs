using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare;

namespace Tests
{
    internal class ServiceProviderSingleton
    {
        private static readonly Lazy<ServiceProviderSingleton> lazy =
            new Lazy<ServiceProviderSingleton>(() => new ServiceProviderSingleton());

        public static ServiceProviderSingleton Instance => lazy.Value;
        public IServiceProvider ServiceProvider;

        private ServiceProviderSingleton()
        {
            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            startup.ConfigureBusinessServices(services);

            // Create service provider
            ServiceProvider = services.BuildServiceProvider();

            // Configure application
            startup.Configure(ServiceProvider, (ILoggerFactory)ServiceProvider.GetService(typeof(ILoggerFactory)));
        }
    }
}
