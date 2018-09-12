using System;
using System.Net;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SpreadShare.BinanceServices
{
    class BinanceTradingService : AbstractTradingService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private BinanceClient _client;
        private long _receiveWindow;

        public BinanceTradingService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<BinanceTradingService>();
            _configuration = configuration;
            _logger.LogInformation("Creating new Binance Client");
        }

        public override void Start()
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
            } else
            {
                _logger.LogCritical($"Authenticated Binance request failed: { result.Error.Message}");
            }
        }

        public override long PlaceMarketOrder(string symbol, OrderSide side)
        {
            var response = _client.PlaceTestOrder("BNBETH", OrderSide.Buy, OrderType.Market, (decimal)0.32, null, null, null, null, null, null, (int)_receiveWindow);
            if (response.Success)
            {
                _logger.LogInformation($"Order {response.Data.OrderId} placement succeeded!");
                return response.Data.OrderId;
            }
            else
            {
                _logger.LogWarning($"Error while placing order: {response.Error.Message}");
                throw new Exception("Order placement failed!");
            }
        }

        public override void CancelOrder(string symbol, long orderId)
        {
            var response = _client.CancelOrder(symbol, orderId, null, null, _receiveWindow);
            if (response.Success)
                _logger.LogInformation($"Order {orderId} succesfully cancelled");
            else
            {
                _logger.LogWarning($"Failed to cancel order {orderId}: {response.Error.Message}");
                throw new Exception("Order cancellation failed!");
            }
        }

        public void QueryOrder(string symbol, long orderId)
        {
            var response = _client.QueryOrder(symbol, orderId, null, _receiveWindow);
            if (response.Success)
            {
               // _logger.LogInformation("Succesfully querried an order");
            } else
            {
                _logger.LogWarning($"Unable to querry order {orderId}: {response.Error.Message}");
            }
        }
    }
}
