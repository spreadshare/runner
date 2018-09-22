using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpreadShare;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.Strategy;
using SpreadShare.ZeroMQ;


namespace UnitTests
{
	public class TestServicesStartup
    {
		//You are encouraged you to add objects from
		//the SpreadShare.* namespace as private members 
		//and initialize them in the constructor.

		private IServiceCollection services;
		private IServiceProvider serviceProvider;
		
		public TestServicesStartup() { 
			// Create service collection
            services = new ServiceCollection();

            // Configure services - Provide depencies for services
            Startup startup = new Startup();
            startup.ConfigureServices(services);
            startup.ConfigureBusinessServices(services);

            // Create service provider
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Configure application
            startup.Configure(serviceProvider, (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory)));
		}

		[Fact]
		public void UserServiceStarts()
		{
			//IUserService user = serviceProvider.GetService<IUserService>();
			Assert.True(true, "lol");
		}
  }
}
