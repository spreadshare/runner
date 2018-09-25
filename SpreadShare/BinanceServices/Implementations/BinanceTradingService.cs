using System;
using System.Linq;
using System.Net;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices.Implementations
{
    internal class BinanceTradingService : AbstractTradingService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private BinanceClient _client;
        private long _receiveWindow;

        public BinanceTradingService(ILoggerFactory loggerFactory, IConfiguration configuration, IUserService userService)
        {
            _logger = loggerFactory.CreateLogger<BinanceTradingService>();
            _configuration = configuration;
            _logger.LogInformation("Creating new Binance Client");
            _userService = userService;
        }

        public override ResponseObject Start()
        {
            //Read the custom receive window, the standard window is often too short.
            _receiveWindow = _configuration.GetValue<long>("BinanceClientSettings:receiveWindow");

            //Enforce the right protocol for the connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _client = new BinanceClient();
            //Read authentication from configuration.
            string apikey = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            _client.SetApiCredentials(apikey, apisecret);

            //Test the connection to binance
            _logger.LogInformation("Testing connection to Binance...");
            var ping = _client.Ping();
            if (ping.Success)
                _logger.LogInformation("Connection to Binance succesful");
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

        public override ResponseObject<long> PlaceMarketOrder(string symbol, OrderSide side, decimal amount)
        {
            var response = _client.PlaceTestOrder("BNBETH", side, OrderType.Market, amount, null, null, null, null, null, null, (int)_receiveWindow);
            if (response.Success)
            {
                _logger.LogInformation($"Order {response.Data.OrderId} placement succeeded!");
                return new ResponseObject<long>(ResponseCodes.Success, response.Data.OrderId);
            }
            else
            {
                _logger.LogWarning($"Error while placing order: {response.Error.Message}");
                return new ResponseObject<long>(ResponseCodes.Error);
            }
        }

        public override ResponseObject CancelOrder(string symbol, long orderId)
        {
            var response = _client.CancelOrder(symbol, orderId, null, null, _receiveWindow);
            if (response.Success) {
                _logger.LogInformation($"Order {orderId} succesfully cancelled");
                return new ResponseObject(ResponseCodes.Success);
            }
            else
            {
                _logger.LogWarning($"Failed to cancel order {orderId}: {response.Error.Message}");
                return new ResponseObject(ResponseCodes.Error, response.Error.Message);
            }
        }

        public override ResponseObject<decimal> GetCurrentPrice(string symbol) {
            var response = _client.GetPrice(symbol);
            if (response.Success) {
                return new ResponseObject<decimal>(ResponseCodes.Success, response.Data.Price);
            } else {
                _logger.LogWarning($"Could not fetch price for {symbol} from binance!");
                return new ResponseObject<decimal>(ResponseCodes.Error);
            }
        }

        public override ResponseObject<decimal> GetPerformancePastHours(string symbol, double hoursBack, DateTime endTime) {
            if (hoursBack <= 0) {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            DateTime startTime = endTime.AddHours(-hoursBack);
            var response = _client.GetKlines(symbol, KlineInterval.OneMinute,startTime, endTime);
            if (response.Success) {
                var length = response.Data.Length;
                var first = response.Data[0].Open;
                var last = response.Data[length - 1].Close;
                return new ResponseObject<decimal>(ResponseCodes.Success, last / first);
            } else {
                _logger.LogCritical(response.Error.Message);
                _logger.LogWarning($"Could not fetch price for {symbol} from binance!");
                return new ResponseObject<decimal>(ResponseCodes.Error);
            }
        }

        public override ResponseObject<Tuple<string, decimal>> GetTopPerformance(double hoursBack, DateTime endTime) {
            if (hoursBack <= 0) {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }
            
            var tradingPairs = _configuration.GetSection("BinanceClientSettings:tradingPairs").AsEnumerable().ToArray();

            decimal max = -1;
            string maxTradingPair = "";

            foreach(var tradingPair in tradingPairs) {
                // GetSection gives a null value
                if  (tradingPair.Value == null) continue;

                var performanceQuery = GetPerformancePastHours(tradingPair.Value, hoursBack, endTime);
                decimal performance;
                if (performanceQuery.Code == ResponseCodes.Success) {
                    performance = performanceQuery.Data;
                } else {
                    return new ResponseObject<Tuple<string, decimal>>(ResponseCodes.Error);
                }

                
                if (max < performance) {
                    max = performance;
                    maxTradingPair = tradingPair.Value;
                }
            }

            return new ResponseObject<Tuple<string, decimal>>(ResponseCodes.Success, new Tuple<string, decimal>(maxTradingPair, max));
        }

    }
}
