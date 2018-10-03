﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using SpreadShare.BinanceServices;
using SpreadShare.BinanceServices.Implementations;
using SpreadShare.Models;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests
{
    public class ListenKeyManagerTests : BaseTest
    {
        public ListenKeyManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

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

        [Fact]
        public void ListenKeyInvalidCredentialsTest()
        {
            var listenKeyManager = SetupInvalidCredentials(10000);

            // Obtain listenKey
            var response = listenKeyManager.Obtain();
            if (!response.Success)
            {
                Logger.LogError("Unable to obtain listenKey");
                return;
            }
            Assert.True(false);
        }

        [Fact]
        public void ListenKeyRenewalTest()
        {
            Console.WriteLine("hoi");
            // Sleep 10 seconds to autorenew twice


            var listenKeyManager = Setup(3000);

            // Obtain listenKey
            var response = listenKeyManager.Obtain();
            if (!response.Success)
            {
                Logger.LogError("Unable to obtain listenKey");
                return;
            }
            // Sleep 10 seconds to autorenew twice

            Thread.Sleep(10000);

        }

        private ListenKeyManager Setup(int interval)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var configuration = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(TestLoggingProvider);

            //Setup the clients
            var client = new BinanceClient();

            // Set credentials
            string apikey = configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = configuration.GetValue<string>("BinanceCredentials:api-secret");
            client.SetApiCredentials(apikey, apisecret);

            return new ListenKeyManager(loggerFactory, client, interval);
        }

        private ListenKeyManager SetupInvalidCredentials(int interval)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
            loggerFactory.AddProvider(TestLoggingProvider);

            //Setup the clients
            var client = new BinanceClient();

            // Set credentials
            string apikey = "myapikey";
            string apisecret = "myapisecret";
            client.SetApiCredentials(apikey, apisecret);

            return new ListenKeyManager(loggerFactory, client, interval);
        }
    }
}
