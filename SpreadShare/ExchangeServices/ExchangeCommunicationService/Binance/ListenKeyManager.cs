using System;
using System.Threading;
using Binance.Net;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance
{
    /// <summary>
    /// Object for obtaining and renewing listen keys for Binance.
    /// </summary>
    internal class ListenKeyManager : IDisposable
    {
        private readonly BinanceClient _client;
        private readonly ILogger _logger;
        private readonly int _interval;

        private string _listenKey;
        private Timer _timer;
        private bool _isRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListenKeyManager"/> class.
        /// Create logger and sets BinanceClient.
        /// </summary>
        /// <param name="loggerFactory">Creates logger.</param>
        /// <param name="client">Creates BinanceClient.</param>
        /// <param name="interval">Renewal interval (default: 30min).</param>
        public ListenKeyManager(ILoggerFactory loggerFactory, BinanceClient client, int interval = 30 * 60 * 1000)
        {
            _logger = loggerFactory.CreateLogger("ListenKeyManager");
            _client = client;
            _interval = interval;
        }

        /// <summary>
        /// Obtain listenkey that is valid for 24 hours (auto renews every 30 min).
        /// </summary>
        /// <returns>ListenKey valid for 24 hours.</returns>
        public ResponseObject<string> Obtain()
        {
            // Cleanup previous instance
            Cleanup();

            // Get listen key
            var getListenKey = _client.StartUserStream();
            if (!getListenKey.Success)
            {
                _logger.LogCritical($"Unable to obtain ListenKey for Binance WebSocket: {getListenKey.Error.Message}");
                return new ResponseObject<string>(ResponseCode.Error);
            }

            _listenKey = getListenKey.Data.ListenKey;

            // Set timer every 30 min for autorenewal
            SetTimer();

            return new ResponseObject<string>(ResponseCode.Success, _listenKey, "Successfully obtained listenKey");
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object's resource.
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _client.Dispose();
            }
        }

        /// <summary>
        /// Clears timer and listenkey.
        /// </summary>
        private void Cleanup()
        {
            // Clear timer
            _timer?.Dispose();

            // Clear listenKey
            _listenKey = null;

            // Clear retry
            _isRetry = false;
        }

        /// <summary>
        /// Set timer for every 30 minutes autorenewal.
        /// </summary>
        private void SetTimer()
        {
            var autoEvent = new AutoResetEvent(true);

            // Set timer to start in 30 min, every 30 min
            _timer = new Timer(Renew, autoEvent, _interval, _interval);
        }

        /// <summary>
        /// Renew listenkey.
        /// </summary>
        /// <param name="stateInfo">Given context from the method starting the timer.</param>
        private void Renew(object stateInfo)
        {
            _logger.LogInformation($"{DateTime.UtcNow} | Requesting renewal of listenKey: {_listenKey}");
            var renewal = _client.KeepAliveUserStream(_listenKey);

            // If renewal error'ed
            if (!renewal.Success)
            {
                _logger.LogError($"{DateTime.UtcNow} | Could not renew listenKey: {_listenKey}");
                _logger.LogError($"Error {renewal.Error.Code}: {renewal.Error.Message}");

                if (!_isRetry)
                {
                    _isRetry = true;
                    Renew(stateInfo);
                }
                else
                {
                    _logger.LogCritical($"ListenKey could not be obtained after a retry at {DateTime.UtcNow}. Expect a disconnect");
                    /* ListenKey will expire in ~60 minutes. It's very likely that the stream has
                     * reached it's 24 hour expiration mark. If the socket is closed, UserService
                     * will request a new key.
                     */
                }
            }

            // Renewal succeeded
            _logger.LogInformation($"{DateTime.UtcNow} | Renewed listenKey: {_listenKey}");
        }
    }
}
