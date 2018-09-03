using System;
using System.Collections.Generic;
using System.Text;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SpreadShare.Services
{
    class BinanceUserService : IUserService
    {
        private BinanceSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public BinanceUserService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _configuration = configuration;
        }


        public void Start()
        {
            //Setup the socket client
            _client = new BinanceSocketClient();
            string apikey = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            _client.SetApiCredentials(apikey, apisecret);

            //Create a temporary client in order to obtain the listen key needed for the socket connection.
            var tempClient = new BinanceClient();
            tempClient.SetApiCredentials(apikey, apisecret);

            string listenKey;
            var getListenKey = tempClient.StartUserStream();
            if (getListenKey.Success)
                listenKey = getListenKey.Data.ListenKey;
            else
            {
                _logger.LogCritical("Unable to obtain Listen Key for binance websocket");
                throw new Exception();
            }


            //Start socket connection
            var succesOrderBook = _client.SubscribeToUserStream(listenKey,
                (accountInfoUpdate) =>
                {

                },
                (orderInfoUpdate) =>
                {

                });
            _logger.LogInformation("Binance User Service was succefully started!");
        }
    }
}
