using System;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Binance implementation of the communication service.
    /// </summary>
    internal class BinanceCommunicationsService : Observable<OrderUpdate>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ListenKeyManager _listenKeyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceCommunicationsService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create a logger to create output.</param>
        public BinanceCommunicationsService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            ReceiveWindow = Configuration.Instance.BinanceClientSettings.ReceiveWindow;

            var auth = Configuration.Instance.BinanceClientSettings.Credentials;
            Client = new BinanceClient();
            Client.SetApiCredentials(auth.Key, auth.Secret);

            var options = new BinanceSocketClientOptions { LogVerbosity = LogVerbosity.Debug };
            Socket = new BinanceSocketClient(options);

            // Setup ListenKeyManager
            _listenKeyManager = new ListenKeyManager(loggerFactory, Client);
        }

        /// <summary>
        /// Gets the number of ticks before timeout should be declared. This value is set in the configuration.
        /// </summary>\
        public long ReceiveWindow { get; }

        /// <summary>
        /// Gets or sets the instance of the binance client.
        /// </summary>
        public virtual IBinanceClient Client { get; protected set; }

        /// <summary>
        /// Gets the instance of the binance user socket.
        /// </summary>
        public BinanceSocketClient Socket { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Enable streams for 24 hours.
        /// </summary>
        public void EnableStreams()
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
            var successOrderBook = Socket.SubscribeToUserStream(
                listenKey,
                accountInfoUpdate =>
                {
                    // TODO: Implement AccountInfoUpdate callback
                },
                orderInfoUpdate =>
                {
                    // ##########################################################################
                    // ####### WARNING ##########################################################
                    // ### Any exception will cause this method to shutdown without warning,
                    // ### causing the observers to hear nothing. This is completely shitty behavior,
                    // ### do not make the mistake I made and waste your time.
                    // ##########################################################################
                    try
                    {
                        _logger.LogDebug(JsonConvert.SerializeObject(orderInfoUpdate));
                        UpdateObservers(BinanceUtilities.ToInternal(orderInfoUpdate));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error parsing a BinanceOrderInfoUpdate: {e.Message} \n {JsonConvert.SerializeObject(orderInfoUpdate)}");
                    }
                });

            if (!successOrderBook.Success)
            {
                _logger.LogError(successOrderBook.Error.Message);
                Program.ExitProgramWithCode(ExitCode.BinanceCommunicationStartupFailure);
            }

            // Set error handler
            successOrderBook.Data.ConnectionLost += () =>
            {
                _logger.LogCritical($"Connection got closed at {DateTime.UtcNow}. Attempt to open socket");
                EnableStreams();
            };

            successOrderBook.Data.ConnectionRestored += t => _logger.LogCritical($"Connection was restored after {t}");

            _logger.LogInformation("Binance Communication Service was successfully started!");
        }

        /// <summary>
        /// Disposes the current object's resource.
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _listenKeyManager?.Dispose();
                Client?.Dispose();
                Socket?.Dispose();
            }
        }
    }
}