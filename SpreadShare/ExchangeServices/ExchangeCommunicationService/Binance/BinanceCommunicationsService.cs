using System;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance
{
    /// <summary>
    /// Binance implementantion of the communication service.
    /// </summary>
    internal class BinanceCommunicationsService : ExchangeCommunications, IDisposable
    {
        /// <summary>
        /// Ticks before timeout should be declared. This value is set in the configuration
        /// </summary>\
        public long ReceiveWindow { get; }
 
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly BinanceCredentials _authy;
        private ListenKeyManager _listenKeyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceCommunicationsService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create a logger to create output</param>
        /// <param name="settings">Used to extract the binance settings</param>
        public BinanceCommunicationsService(ILoggerFactory loggerFactory, SettingsService settings)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
            _authy = settings.BinanceSettings.Credentials;
            ReceiveWindow = settings.BinanceSettings.ReceiveWindow;
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

        /// <inheritdoc />
        protected override void Startup()
        {
            Client = new BinanceClient();
            Client.SetApiCredentials(_authy.Key, _authy.Secret);

            var options = new BinanceSocketClientOptions { LogVerbosity = LogVerbosity.Debug };
            Socket = new BinanceSocketClient(options);

            // Setup ListenKeyManager
            _listenKeyManager = new ListenKeyManager(_loggerFactory, Client);

            EnableStreams();
        }

        /// <summary>
        /// Enable streams for 24 hours
        /// </summary>
        private void EnableStreams()
        {
            _logger.LogInformation($"Enabling streams at {DateTime.UtcNow}");

            // Obtain listenKey
            var response = _listenKeyManager.Obtain();
            if (!response.Success)
            {
                _logger.LogError("Unable to obtain listenKey");
                return;
            }

            var listenKey = response.Data;

            // Start socket connection
            // TODO: Is this correct?
            // TODO: TradeId is not correct.
            var succesOrderBook = Socket.SubscribeToUserStream(
                listenKey,
                accountInfoUpdate =>
                {
                    // TODO: Implement AccountInfoUpdate callback
                },
                orderInfoUpdate => UpdateObservers(new OrderUpdate(
                    orderInfoUpdate.OrderId,
                    0,
                    BinanceUtilities.ToInternal(orderInfoUpdate.Type),
                    DateTimeOffset.FromFileTime(orderInfoUpdate.OrderCreationTime.ToFileTime()).ToUnixTimeMilliseconds(),
                    orderInfoUpdate.Price,
                    BinanceUtilities.ToInternal(orderInfoUpdate.Side),
                    TradingPair.Parse(orderInfoUpdate.Symbol),
                    orderInfoUpdate.Quantity)
                {
                    Status = BinanceUtilities.ToInternal(orderInfoUpdate.Status)
                }));

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

            _logger.LogInformation("Binance Communication Service was successfully started!");
        }
    }
}