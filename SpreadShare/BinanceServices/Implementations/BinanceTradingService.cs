using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.BinanceServices.Implementations
{
    internal class BinanceTradingService : AbstractTradingService
    {
        private readonly ILogger _logger;
        private readonly SettingsService _settings;
        private readonly AbstractUserService _userService;
        private BinanceClient _client;
        private long _receiveWindow;
        private ConcurrentDictionary<long, TradeState> _orderStatus;

        public BinanceTradingService(ILoggerFactory loggerFactory, ISettingsService settings, IUserService userService)
        {
            _logger = loggerFactory.CreateLogger<BinanceTradingService>();
            _settings = settings as SettingsService;
            _logger.LogInformation("Creating new Binance Client");
            _userService = userService as AbstractUserService;
            _orderStatus = new ConcurrentDictionary<long, TradeState>();

            //Subscribe to orderUpdates
            _userService.OrderUpdateHandler += OnOrderUpdate;
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

        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            var query = _userService.GetPortfolio();
            if (!query.Success) return new ResponseObject(ResponseCodes.Error, "Could not retreive assets");
            //TODO
            //Implement pairs
            //Implement LOT_SIZE per pair.
            decimal amount = query.Data.GetFreeBalance(pair.Left);
            amount = Math.Floor(amount*100) / 100;
            _logger.LogInformation($"About to place a {side} order for {amount}{pair}.");

            var response = _client.PlaceOrder("BNBETH", side, OrderType.Market, amount, null, null, null, null, null, null, (int)_receiveWindow);
            if (response.Success)
                _logger.LogInformation($"Order {response.Data.OrderId} request succeeded!");
            else
            {
                _logger.LogWarning($"Error while placing order: {response.Error.Message}");
                return new ResponseObject(ResponseCodes.Error, response.Error.Message);
            }

            int msTicker = 0;
            TradeState state;
            while(true) {
                state = _orderStatus.GetOrAdd(response.Data.OrderId, TradeState.Unknown);
                if (state == TradeState.Executed) {
                    return new ResponseObject(ResponseCodes.Success);
                }
                if (state == TradeState.Canceled | state == TradeState.Rejected) {
                    return new ResponseObject(ResponseCodes.Error, $"Trade was not executed, reason: {state}");
                }

                //Wait for a maximum of 10 seconds
                if (msTicker < 10000) {
                    Thread.Sleep(1);
                } else {
                    break;
                }
            }
            return new ResponseObject(ResponseCodes.Error, $"Trade was not confirmed in time, last state: {state}");
        }

        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            throw new NotImplementedException();
        }

        public override ResponseObject<decimal> GetCurrentPrice(Currency symbol) {
            var response = _client.GetPrice(symbol.ToString());
            if (response.Success) {
                return new ResponseObject<decimal>(ResponseCodes.Success, response.Data.Price);
            } else {
                _logger.LogWarning($"Could not fetch price for {symbol} from binance!");
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
            } else {
                _logger.LogCritical(response.Error.Message);
                _logger.LogWarning($"Could not fetch price for {pair} from binance!");
                return new ResponseObject<decimal>(ResponseCodes.Error);
            }
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
                return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCodes.Error, "No trading pairs defined");

            return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCodes.Success, new Tuple<CurrencyPair, decimal>(maxTradingPair, max));
        }


        public void OnOrderUpdate(object sender, BinanceStreamOrderUpdate order) {
            _logger.LogInformation($"Order update | id {order.OrderId}, executionType {order.ExecutionType}");
            TradeState state;
            switch(order.ExecutionType) {
                case ExecutionType.New: state = TradeState.Received; break;
                case ExecutionType.Canceled: state = TradeState.Canceled; break;
                case ExecutionType.Expired: state = TradeState.Expired; break;
                case ExecutionType.Rejected: state = TradeState.Rejected; break;
                case ExecutionType.Trade: state = TradeState.Executed; break;
                default: _logger.LogCritical($"Unknown Execution Type received from Binance: {order.ExecutionType}"); return;
            }

            //The function is used to generate a value when the key was already present, in this case it should
            //just update the value as is.
            _orderStatus.AddOrUpdate(order.OrderId, state, (p,q) => state);
        }

    }
}