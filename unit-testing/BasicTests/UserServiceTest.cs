using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using Xunit;

namespace UnitTests
{
    public class UserServiceTest : BaseTest
    {
        [Fact]
        public void TestMessage()
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var userService = (BinanceUserService)serviceProvider.GetService<IUserService>();
            userService.Start();
            userService.OrderUpdateHandler += (sender, update) =>
            {
                Console.WriteLine(update.Time);
            };
            while (true)
            {
                var x = userService.GetPortfolio();
                Console.WriteLine(x.Success);
                Console.WriteLine(x.Code);
                Thread.Sleep(3600000);
            }
        }
    }
}
