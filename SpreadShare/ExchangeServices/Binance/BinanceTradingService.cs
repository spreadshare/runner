using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.ExchangeServices.Binance
{
    /// <summary>
    /// Service responsible for trading in Binance
    /// </summary>
    internal class BinanceTradingService : AbstractTradingService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly BinanceSettings _binanceSettings;
        private readonly AbstractUserService _userService;
        private BinanceClient _client;
        private long _receiveWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceTradingService"/> class.
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        /// <param name="settings">Settings service</param>
        /// <param name="userService">User service for accessing the portfolio</param>
        public BinanceTradingService(ILoggerFactory loggerFactory, ISettingsService settings, IUserService userService)
        {
            _logger = loggerFactory.CreateLogger<BinanceTradingService>();
            _binanceSettings = (settings as SettingsService).BinanceSettings;
            _logger.LogInformation("Creating new Binance Client");
            _userService = userService as AbstractUserService;
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns>A response object that indicates the result of the service</returns>
        public override ResponseObject Start()
        {
            // Read the custom receive window, the standard window is often too short.
            _receiveWindow = _binanceSettings.ReceiveWindow;

            // Enforce the right protocol for the connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _client = new BinanceClient();

            // Read authentication from configuration.
            string apikey = _binanceSettings.Credentials.Key;
            string apisecret = _binanceSettings.Credentials.Secret;
            _client.SetApiCredentials(apikey, apisecret);

            // Test the connection to binance
            _logger.LogInformation("Testing connection to Binance...");
            var ping = _client.Ping();
            if (ping.Success)
            {
                _logger.LogInformation("Connection to Binance succesfull");
            }
            else
            {
                _logger.LogCritical($"Connection to binance failed: no response ==> {ping.Error.Message}");
            }

            // Test the credentials by retrieving the account information
            var result = _client.GetAccountInfo(_receiveWindow);
            if (result.Success)
            {
                _logger.LogInformation("Binance account info:");
                foreach (BinanceBalance balance in result.Data.Balances)
                {
                    if (balance.Total > 0)
                    {
                        _logger.LogInformation($"{balance.Total} {balance.Asset} (free: {balance.Free} - locked: {balance.Locked})");
                    }
                }

                return new ResponseObject(ResponseCode.Success);
            }

            return new ResponseObject(ResponseCode.Error, $"Authenticated Binance request failed: {result.Error.Message}");
        }

        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">Currency pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            var query = _userService.GetPortfolio();
            if (!query.Success)
            {
                return new ResponseObject(ResponseCode.Error, "Could not retrieve assets");
            }

            decimal correction = 1.0M;

            while (correction > 0.95M)
            {
                decimal amount = query.Data.GetFreeBalance(side == OrderSide.Buy ? pair.Right : pair.Left);

                // The amount should be expressed in the base pair.
                if (side == OrderSide.Buy)
                {
                    var priceQuery = GetCurrentPriceTopAsk(pair);
                    if (priceQuery.Success)
                    {
                        // Ensure that the price stay valid for a short while.
                        amount = (amount / priceQuery.Data) * correction;
                        _logger.LogInformation($"Current price of {pair} is {priceQuery.Data}{pair.Right}");
                    }
                    else
                    {
                        return new ResponseObject(ResponseCode.Error, priceQuery.ToString());
                    }
                }

                _logger.LogInformation($"Pre rounded amount {amount}{pair.Left}");
                amount = pair.RoundToTradable(amount);
                _logger.LogInformation($"About to place a {side.ToString()} order for {amount}{pair.Left}.");

                var trade = _client.PlaceOrder(pair.ToString(), side, OrderType.Market, amount, null, null, null, null, null, null, (int)_receiveWindow);
                if (trade.Success)
                {
                    _logger.LogInformation($"Order {trade.Data.OrderId} request succeeded! pending confirmation...");
                    return WaitForOrderFilledConfirmation(pair, trade.Data.OrderId);
                }

                _logger.LogWarning($"Error while placing order: {trade.Error.Message}");
                correction -= 0.01M;
            }

            return new ResponseObject(ResponseCode.Error, $"Market order failed, even after underestimating wth a factor of {correction}");
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public override ResponseObject CancelOrder(long orderId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair)
        {
            var response = _client.GetPrice(pair.ToString());
            if (!response.Success)
            {
                _logger.LogWarning($"Could not fetch price for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            return new ResponseObject<decimal>(ResponseCode.Success, response.Data.Price);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair)
        {
            var response = _client.GetOrderBook(pair.ToString());
            if (!response.Success)
            {
                _logger.LogWarning($"Could not fetch top bid for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            decimal ret = response.Data.Bids.Max(x => x.Price);
            return new ResponseObject<decimal>(ResponseCode.Success, ret);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair)
        {
            var response = _client.GetOrderBook(pair.ToString());
            if (!response.Success)
            {
                _logger.LogWarning($"Could not fetch top ask for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            decimal ret = response.Data.Asks.Max(x => x.Price);
            return new ResponseObject<decimal>(ResponseCode.Success, ret);
        }

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">Currency pair to obtain performance of</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>A response object with the performance on success</returns>
        public override ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            DateTime startTime = endTime.AddHours(-hoursBack);
            var response = _client.GetKlines(pair.ToString(), KlineInterval.OneMinute, startTime, endTime);

            if (response.Success)
            {
                var length = response.Data.Length;
                var first = response.Data[0].Open;
                var last = response.Data[length - 1].Close;
                return new ResponseObject<decimal>(ResponseCode.Success, last / first);
            }

            _logger.LogCritical(response.Error.Message);
            _logger.LogWarning($"Could not fetch price for {pair} from binance!");
            return new ResponseObject<decimal>(ResponseCode.Error);
        }

        /// <summary>
        /// Gets the top performing currency pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>Top performing currency pair</returns>
        public override ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            decimal max = -1;
            CurrencyPair maxTradingPair = null;

            foreach (var tradingPair in pairs)
            {
                var performanceQuery = GetPerformancePastHours(tradingPair, hoursBack, endTime);
                decimal performance;
                if (performanceQuery.Code == ResponseCode.Success)
                {
                    performance = performanceQuery.Data;
                }
else
                {
                    _logger.LogWarning($"Error fetching performance data: {performanceQuery}");
                    return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Error, performanceQuery.ToString());
                }

                if (max < performance)
                {
                    max = performance;
                    maxTradingPair = tradingPair;
                }
            }

            if (maxTradingPair == null)
            {
                return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Error, "No trading pairs defined");
            }

            return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Success, new Tuple<CurrencyPair, decimal>(maxTradingPair, max));
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
                _client.Dispose();
            }
        }

        /// <summary>
        /// Wait for the confirmation that the order has been filled
        /// </summary>
        /// <param name="pair">Currency pair to obtain the order of</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        private ResponseObject WaitForOrderFilledConfirmation(CurrencyPair pair, long orderId)
        {
            int attempts = 0;
            OrderStatus state = OrderStatus.New;
            while (true)
            {
                // The only way to confirm an order has been filled is using the public endpoint.
                var orderQuery = _client.QueryOrder(pair.ToString(), orderId, null, _binanceSettings.ReceiveWindow);

                if (orderQuery.Success)
                {
                    state = orderQuery.Data.Status;
                    if (state == OrderStatus.Filled)
                    {
                        return new ResponseObject(ResponseCode.Success);
                    }
                }

                // Try a maximum of 20 times.
                if (++attempts < 20)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    break;
                }
            }

            return new ResponseObject(ResponseCode.Error, $"Trade was not filled or queried in time ({attempts} attempts), last state: {state}");
        }
    }
}