using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices.Implementations
{
    /// <summary>
    /// Service responsible for fetching the portfolio and watching orders
    /// </summary>
    internal class BinanceUserService : AbstractUserService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private BinanceClient _client;
        private BinanceSocketClient _socketclient;
        private ListenKeyManager _listenKeyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceUserService"/> class.
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        /// <param name="configuration">Configuration of the activity</param>
        public BinanceUserService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(GetType());
            _configuration = configuration;
        }

        /// <summary>
        /// Start the BinanceUserService, will configure callback functions.
        /// </summary>
        /// <returns>Whether the service was started successfully</returns>
        public override ResponseObject Start()
        {
            // Setup the clients
            _client = new BinanceClient();
            var options = new BinanceSocketClientOptions { LogVerbosity = LogVerbosity.Debug };
            _socketclient = new BinanceSocketClient(options);

            // Set credentials
            string apikey = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            _client.SetApiCredentials(apikey, apisecret);

            // Setup ListenKeyManager
            _listenKeyManager = new ListenKeyManager(_loggerFactory, _client);

            // Setup streams
            return EnableStreams();
        }

        /// <summary>
        /// Gets the portfolio of the user
        /// </summary>
        /// <returns>The portfolio</returns>
        public override ResponseObject<Assets> GetPortfolio()
        {
            var accountInfo = _client.GetAccountInfo();
            if (!accountInfo.Success)
            {
                _logger.LogCritical($"Could not get assets: {accountInfo.Error.Message}");
                return new ResponseObject<Assets>(ResponseCodes.Error);
            }

            // Map to general ExchangeBalance datatype for parsing to assets object.
            var values = accountInfo.Data.Balances.Select(x => new ExchangeBalance(x.Asset, x.Free, x.Locked)).ToList();

            return new ResponseObject<Assets>(ResponseCodes.Success, new Assets(values));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object's resource
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
                _loggerFactory.Dispose();
                _socketclient.Dispose();
            }
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

            // Start socket connection
            var succesOrderBook = _socketclient.SubscribeToUserStream(
                listenKey,
                accountInfoUpdate =>
                {
                    // TODO: Implement AccountInfoUpdate callback
                },
                orderInfoUpdate =>
                {
                    // TODO: Information not currently used.
                });

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
    }
}
