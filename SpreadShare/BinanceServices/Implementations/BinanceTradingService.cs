﻿using System;
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

        protected override long PlaceMarketOrder(string symbol, OrderSide side, decimal amount)
        {
            var response = _client.PlaceTestOrder("BNBETH", side, OrderType.Market, amount, null, null, null, null, null, null, (int)_receiveWindow);
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

        public override ResponseObject ChangeEntirePosition(string symbol) {
            var assets = _userService.GetPortfolio();
            return new ResponseObject(ResponseCodes.NotDefined);
        } 

        public override decimal GetPrice(string symbol) {
            var response = _client.GetPrice(symbol);
            if (response.Success) {
                return response.Data.Price;
            } else {
                _logger.LogWarning($"Could not fetch price for {symbol} from binance!");
                return 0;
            }
        }
    }
}
