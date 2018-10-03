using System;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Strategy;

namespace SpreadShare.BinanceServices.Implementations
{
    internal class BinanceUserService : AbstractUserService
    {
        private BinanceClient _client;
        private BinanceSocketClient _socketclient;
        private ListenKeyManager _listenKeyManager;

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public BinanceUserService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(GetType());
            _configuration = configuration;
        }

        /// <summary>
        /// Start the BinanceUserService, will configure callback functions.
        /// </summary>
        public override ResponseObject Start()
        {
            //Setup the clients
            _client = new BinanceClient();
            var options = new BinanceSocketClientOptions { LogVerbosity = LogVerbosity.Debug };
            _socketclient = new BinanceSocketClient(options);

            // Set credentials
            string apikey = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            _client.SetApiCredentials(apikey, apisecret);

            // Get listen key
            _listenKeyManager = new ListenKeyManager(_loggerFactory, _client);

            // Setup streams
            return EnableStreams();
        }


        /// <summary>
        /// Enable streams for 24 hours
        /// </summary>
        /// <returns>If this operation succeeded</returns>
        private ResponseObject EnableStreams()
        {
            _logger.LogInformation($"Enabling streams at {DateTime.UtcNow}");

            // Obtain listenKey
            var response = _listenKeyManager.Obtain();
            if (!response.Success)
            {
                _logger.LogError("Unable to obtain listenKey");
                return new ResponseObject(ResponseCodes.Error);
            }
            var listenKey = response.Data;

            //Start socket connection
            var succesOrderBook = _socketclient.SubscribeToUserStream(
                listenKey,
                accountInfoUpdate =>
                {
                    // Not implemented
                },
                OnOrderUpdate);

            // Set error handlers
            succesOrderBook.Data.Closed += () =>
            {
                _logger.LogCritical($"Connection got closed at {DateTime.UtcNow}. Attempt to open socket");
                EnableStreams();
            };
            succesOrderBook.Data.Error += e =>
            {
                _logger.LogError($"Connection got error at {DateTime.UtcNow}: {e}");
                EnableStreams();
            };

            _logger.LogInformation("Binance User Service was successfully started!");
            return new ResponseObject(ResponseCodes.Success);
        }

        public override ResponseObject<Assets> GetPortfolio()
        {
            var accountInfo = _client.GetAccountInfo();
            if (!accountInfo.Success) {
                _logger.LogCritical($"Could not get assets: {accountInfo.Error.Message}");
                return new ResponseObject<Assets>(ResponseCodes.Error);
            }
            return new ResponseObject<Assets>(ResponseCodes.Success, new Assets(accountInfo.Data.Balances));
        }
    }
}
