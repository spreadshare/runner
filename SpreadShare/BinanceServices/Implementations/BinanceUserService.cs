using System;
using System.Threading.Tasks;
using Binance.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Strategy;

namespace SpreadShare.BinanceServices.Implementations
{
    internal class BinanceUserService : AbstractUserService
    {
        private BinanceClient _client;
        private BinanceSocketClient _socketclient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;


        public BinanceUserService(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _configuration = configuration;
        }

        /// <summary>
        /// Start the BinanceUserService, will configure callback functions.
        /// </summary>
        public override ResponseObject Start()
        {
            //Setup the clients
            _client = new BinanceClient();
            _socketclient = new BinanceSocketClient();
            string apikey = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string apisecret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            _client.SetApiCredentials(apikey, apisecret);
            _socketclient.SetApiCredentials(apikey, apisecret);

            string listenKey;
            var getListenKey = _client.StartUserStream();
            if (getListenKey.Success)
                listenKey = getListenKey.Data.ListenKey;
            else
            {
                _logger.LogCritical($"Unable to obtain Listen Key for binance websocket: {getListenKey.Error.Message}");
                return new ResponseObject(ResponseCodes.Error);
            }


            //Start socket connection
            var succesOrderBook = _socketclient.SubscribeToUserStream(listenKey,
                (accountInfoUpdate) =>
                {

                },
                (orderInfoUpdate) =>
                {
                    OnOrderUpdate(orderInfoUpdate);
                });
            _logger.LogInformation("Binance User Service was succesfully started!");

            return new ResponseObject(ResponseCodes.Success);
        }

        public override Assets GetPortfolio()
        {
            var accountInfo = _client.GetAccountInfo();
            if (!accountInfo.Success) {
                throw new Exception($"Could not get acccount info: {accountInfo.Error}");
            }
            return new Assets(accountInfo.Data.Balances);
        }
    }
}
