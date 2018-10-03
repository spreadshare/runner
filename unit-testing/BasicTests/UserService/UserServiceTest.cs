using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class UserServiceTest : BaseTest
    {
        public UserServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

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
