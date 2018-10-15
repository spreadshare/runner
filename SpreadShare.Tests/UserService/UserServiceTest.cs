using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.UserService
{
    /// <summary>
    /// Tests of the <UserService cref="UserService"/> class
    /// </summary>
    public class UserServiceTest : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTest"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        public UserServiceTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests if user service stays alive for 1 hour
        /// </summary>
        [Fact(Skip = "Tests take 1 hour")]
        public void TestMessage()
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var userService = (BinanceUserService)serviceProvider.GetService<IUserService>();
            userService.Start();
            while (true)
            {
                var x = userService.GetPortfolio();
                Console.WriteLine(x.Success);
                Console.WriteLine(x.Codes);
                Thread.Sleep(3600000);
            }
        }
    }
}
