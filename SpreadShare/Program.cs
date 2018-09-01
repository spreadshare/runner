using System;
using SpreadShare.Services;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.DependencyInjection;

namespace SpreadShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create service collection
            IServiceCollection services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            startup.Configure(serviceProvider);

            // --------------------------------------------------
            // Setup finished --> Execute business logic services
            ExecuteBusinessLogic(serviceProvider);
        }

        private static void ExecuteBusinessLogic(IServiceProvider serviceProvider)
        {
            // Get Service and call method
            var service = serviceProvider.GetService<IGetExchangeData>();
            service.Connect();

            // TODO: Find more suitable way to manage application flow and keep it running
            Console.ReadLine();
        }
    }
}
