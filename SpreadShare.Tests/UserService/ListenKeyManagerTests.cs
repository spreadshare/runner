using System;
using System.Linq;
using System.Threading;
using Binance.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices.Implementations;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.UserService
{
    /// <summary>
    /// Tests of the <ListenKeyManager cref="ListenKeyManager"/> class
    /// </summary>
    public class ListenKeyManagerTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListenKeyManagerTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput</param>
        public ListenKeyManagerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Tests if a ListenKey can be obtained
        /// Assumption: Correct API Credentials
        /// </summary>
        [Fact]
        public void ListenKeyObtainTest()
        {
            var listenKeyManager = Setup(10000);

            // Obtain listenKey
            var response = listenKeyManager.Obtain();
            if (!response.Success)
            {
                Logger.LogError("Unable to obtain listenKey");
                Assert.True(false);
            }

            var listenKey = response.Data;
            Logger.LogInformation($"ListenKey obtained: {listenKey}");
            Assert.True(listenKey != null & listenKey.Length > 10);
        }

        /// <summary>
        /// Validates that an error response is given when the API
        /// Credentials are wrong
        /// </summary>
        [Fact]
        public void ListenKeyInvalidCredentialsTest()
        {
            var listenKeyManager = SetupInvalidCredentials(10000);

            // Obtain listenKey
            var response = listenKeyManager.Obtain();
            if (response.Success)
            {
                Logger.LogError("I should not be able to obtain a listenKey");
                Assert.True(false);
            }
        }

        /// <summary>
        /// Tests whether the listenkey is automatically renewed
        /// </summary>
        [Fact]
        public void ListenKeyRenewalTest()
        {
            var listenKeyManager = Setup(3000);

            // Obtain listenKey
            var response = listenKeyManager.Obtain();
            if (!response.Success)
            {
                Logger.LogError("Unable to obtain listenKey");
                return;
            }

            // Sleep 10 seconds to autorenew three times
            Thread.Sleep(10000);
            var j = TestLoggingProvider.Messages.Count(message => message.Contains("Renewed listenKey", StringComparison.InvariantCulture));
            if (j < 2)
            {
                Assert.True(false);
            }
        }

        /// <summary>
        /// Create a valid ListenKeyManager
        /// </summary>
        /// <param name="interval">Autorenewal interval</param>
        /// <returns>Valid ListenKeyManager</returns>
        private static ListenKeyManager Setup(int interval)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var configuration = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));

            // Setup the clients
            var client = new BinanceClient();

            // Set credentials
            string apikey = configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = configuration.GetValue<string>("BinanceCredentials:api-secret");
            client.SetApiCredentials(apikey, apisecret);

            return new ListenKeyManager(loggerFactory, client, interval);
        }

        /// <summary>
        /// Create an invalid ListenKeyManager (wrong credentials)
        /// </summary>
        /// <param name="interval">Autorenewal interval</param>
        /// <returns>ListenKeyManager with incorrect credentials</returns>
        private static ListenKeyManager SetupInvalidCredentials(int interval)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));

            // Setup the clients
            var client = new BinanceClient();

            // Set credentials
            const string apikey = "myapikey";
            const string apisecret = "myapisecret";
            client.SetApiCredentials(apikey, apisecret);

            return new ListenKeyManager(loggerFactory, client, interval);
        }
    }
}
