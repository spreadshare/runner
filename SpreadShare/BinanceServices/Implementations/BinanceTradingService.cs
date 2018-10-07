using System;
using System.Globalization;
using System.Net;
using System.Threading;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.BinanceServices.Implementations
{
    internal class BinanceTradingService : AbstractTradingService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly SettingsService _settings;
        private readonly AbstractUserService _userService;
        private BinanceClient _client;
        private long _receiveWindow;

        public BinanceTradingService(ILoggerFactory loggerFactory, ISettingsService settings, IUserService userService)
        {
            _logger = loggerFactory.CreateLogger<BinanceTradingService>();
            _settings = settings as SettingsService;
            _logger.LogInformation("Creating new Binance Client");
            _userService = userService as AbstractUserService;
        }

        public override ResponseObject Start()
        {
            //Read the custom receive window, the standard window is often too short.
            _receiveWindow = _settings.BinanceSettings.ReceiveWindow;

            //Enforce the right protocol for the connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _client = new BinanceClient();
            //Read authentication from configuration.
            string apikey = _settings.BinanceSettings.Credentials.Key;
            string apisecret = _settings.BinanceSettings.Credentials.Secret;
            _client.SetApiCredentials(apikey, apisecret);

            //Test the connection to binance
            _logger.LogInformation("Testing connection to Binance...");
            var ping = _client.Ping();
            if (ping.Success)
                _logger.LogInformation("Connection to Binance succesfull");
            else
                _logger.LogCritical($"Connection to binance failed: no response ==> {ping.Error.Message}");


            //Test the credentials by retrieving the account information
            var result = _client.GetAccountInfo(_receiveWindow);
            if (result.Success)
            {
                _logger.LogInformation("Binance account info:");
                foreach (BinanceBalance balance in result.Data.Balances)
                    if (balance.Total > 0)
                        _logger.LogInformation($"{balance.Total} {balance.Asset} (free: {balance.Free} - locked: {balance.Locked})");
                return new ResponseObject(ResponseCodes.Success);
            } else
            {
               return new ResponseObject(ResponseCodes.Error, $"Authenticated Binance request failed: { result.Error.Message}");
            }
        }

        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            var query = _userService.GetPortfolio();
            if (!query.Success) return new ResponseObject(ResponseCodes.Error, "Could not retreive assets");
 
            decimal correction = 1.0M;

            while(correction > 0.95M)
            {
                decimal amount = query.Data.GetFreeBalance(side == OrderSide.Buy ? pair.Right : pair.Left);
                //The amount should be expressed in the base pair.
                if (side == OrderSide.Buy) {
                    var priceQuery = GetCurrentPrice(pair);
                    if (priceQuery.Success) {
                        _logger.LogInformation($"Current price of {pair} is {priceQuery.Data}{pair.Right}");
                        amount = (amount / priceQuery.Data) * correction; //ensure that the price stay valid for a short while.
                    } else {
                        return new ResponseObject(ResponseCodes.Error, priceQuery.ToString());
                    }
                }
                _logger.LogInformation($"Pre rounded amount {amount}{pair.Left}");
                amount = pair.RoundToTradable(amount);
                _logger.LogInformation($"About to place a {side.ToString()} order for {amount}{pair.Left}.");

                var trade = _client.PlaceOrder(pair.ToString(), side, OrderType.Market, amount, null, null, null, null, null, null, (int)_receiveWindow);
                if (trade.Success) {
                    _logger.LogInformation($"Order {trade.Data.OrderId} request succeeded! pending confirmation...");
                    return WaitForOrderFilledConfirmation(pair, trade.Data.OrderId);      
                }
                else
                {
                    _logger.LogWarning($"Error while placing order: {trade.Error.Message}");
                    correction -= 0.01M;
                }
            }
            return new ResponseObject(ResponseCodes.Error, $"Market order failed, even after underestimating wth a factor of {correction}");  
        }

        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            throw new NotImplementedException();
        }

        public override ResponseObject<decimal> GetCurrentPrice(CurrencyPair pair) {
            var response = _client.GetPrice(pair.ToString());
            if (response.Success) {
                return new ResponseObject<decimal>(ResponseCodes.Success, response.Data.Price);
            } else {
                _logger.LogWarning($"Could not fetch price for {pair} from binance!");
                return new ResponseObject<decimal>(ResponseCodes.Error);
            }
        }

        public override ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime) {
            if (hoursBack <= 0) {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }
            DateTime startTime = endTime.AddHours(-hoursBack);
            var response = _client.GetKlines(pair.ToString(), KlineInterval.OneMinute,startTime, endTime);

            if (response.Success) {
                var length = response.Data.Length;
                var first = response.Data[0].Open;
                var last = response.Data[length - 1].Close;
                return new ResponseObject<decimal>(ResponseCodes.Success, last / first);
            }

            _logger.LogCritical(response.Error.Message);
            _logger.LogWarning($"Could not fetch price for {pair} from binance!");
            return new ResponseObject<decimal>(ResponseCodes.Error);
        }

        public override ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(double hoursBack, DateTime endTime) {
            if (hoursBack <= 0) {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }
            
            decimal max = -1;
            CurrencyPair maxTradingPair = null;

            foreach(var tradingPair in _settings.ActiveTradingPairs) {
                var performanceQuery = GetPerformancePastHours(tradingPair, hoursBack, endTime);
                decimal performance;
                if (performanceQuery.Code == ResponseCodes.Success) {
                    performance = performanceQuery.Data;
                } else {
                    _logger.LogWarning($"Error fetching performance data: {performanceQuery}");
                    return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCodes.Error, performanceQuery.ToString());
                }

                
                if (max < performance) {
                    max = performance;
                    maxTradingPair = tradingPair;
                }
            }

            if (maxTradingPair == null)
            {
                return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCodes.Error, "No trading pairs defined");
            }

            return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCodes.Success, new Tuple<CurrencyPair, decimal>(maxTradingPair, max));
        }

        private ResponseObject WaitForOrderFilledConfirmation(CurrencyPair pair, long orderId)
        {
            int attempts = 0;
            OrderStatus state = OrderStatus.New;
            while(true) {
                //The only way to confirm an order has been filled is using the public endpoint.
                var orderQuery = _client.QueryOrder(pair.ToString(), orderId, null, _settings.BinanceSettings.ReceiveWindow);

                if (orderQuery.Success) {
                    state = orderQuery.Data.Status;
                    if (state == OrderStatus.Filled)
                        return new ResponseObject(ResponseCodes.Success);
                }

                //Try a maximum of 20 times.
                if (++attempts < 20) {
                    Thread.Sleep(500);
                } else {
                    break;
                }
            }
            return new ResponseObject(ResponseCodes.Error, $"Trade was not filled or queried in time ({attempts} attempts), last state: {state}");
        }
        
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}