using System;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Binance;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance
{
    /// <summary>
    /// Binance implementantion of the communication service.
    /// </summary>
    internal class BinanceCommunicationsService : IExchangeCommunicationService, IDisposable
    {
        private readonly BinanceCredentials _authy;
        private readonly BinanceSettings _settings;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private ListenKeyManager _listenKeyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceCommunicationsService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create a logger to create output</param>
        /// <param name="settings">Used to extract the binance settings</param>
        public BinanceCommunicationsService(ILoggerFactory loggerFactory, ISettingsService settings)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(GetType());
            _settings = ((SettingsService)settings).BinanceSettings;
            _authy = _settings.Credentials;
        }

        /// <summary>
        /// Gets the instance of the binance client
        /// </summary>
        public BinanceClient Client { get; private set; }

        /// <summary>
        /// Gets the instance of the binance user socket
        /// </summary>
        public BinanceSocketClient Socket { get; private set; }

        /// <inheritdoc />
        public ResponseObject Start()
        {
            Client = new BinanceClient();
            Client.SetApiCredentials(_authy.Key, _authy.Secret);

            var options = new BinanceSocketClientOptions { LogVerbosity = LogVerbosity.Debug };
            Socket = new BinanceSocketClient(options);

            // Setup ListenKeyManager
            _listenKeyManager = new ListenKeyManager(_loggerFactory, Client);

            return EnableStreams();
        }

        /// <inheritdoc />
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
                _listenKeyManager?.Dispose();
                Client?.Dispose();
                Socket?.Dispose();
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
                return new ResponseObject(ResponseCode.Error);
            }

            var listenKey = response.Data;

            // Start socket connection
            var succesOrderBook = Socket.SubscribeToUserStream(
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
            return new ResponseObject(ResponseCode.Success);
        }
    }
}