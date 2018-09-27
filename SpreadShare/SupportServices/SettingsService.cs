using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    public class SettingsService : ISettingsService
    {
        IConfiguration _configuration;
        List<CurrencyPair> _tradingPairs;
        BinanceSettings _binanceSettings;

        ITradingService _tradingService;
        ILogger _logger;
        public SettingsService(IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            _configuration = Configuration;
            _tradingPairs = new List<CurrencyPair>();
            _logger = loggerFactory.CreateLogger<SettingsService>();
            using(var client = new BinanceClient())
            {
                Regex rx = new Regex("(.*)(BTC|ETH|USDT|BNB)",
                     RegexOptions.Compiled | RegexOptions.IgnoreCase);


                var listQuery = client.GetAllPrices();
                if (listQuery.Success) {
                    foreach(var item in listQuery.Data) {
                        Console.WriteLine(item.Symbol);
                        var pairs = rx.Matches(item.Symbol);
                        foreach(var pair in pairs.Reverse()) {
                            if (!pair.Success) continue;
                            string left = pair.Groups[1].Value;
                            string right = pair.Groups[2].Value;
                        }
                    }
                }
            }
        }
        public ResponseObject Start()
        {
            try {
                ReadTradingPairs();
                ReadBinanceSettings();
            } catch(Exception e) {
                return new ResponseObject(ResponseCodes.Error, e.Message);
            }
            return new ResponseObject(ResponseCodes.Success);
        }

        private void ReadBinanceSettings() {
            string key = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string secret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            Authy authy = new Authy(key, secret);

            long receiveWindow = _configuration.GetValue<long>("BinanceClientSettings:receiveWindow");
            _binanceSettings = new BinanceSettings(authy, receiveWindow);

        }

        private void ReadTradingPairs() {
            var tradingPairs = _configuration.GetSection("BinanceClientSettings:tradingPairs").AsEnumerable().ToArray();
            foreach(var tradingPair in tradingPairs)
            {
                CurrencyPair pair;
                try {
                    pair = CurrencyPair.Parse(tradingPair.Value);
                } catch(Exception e) {
                    _logger.LogInformation(e.Message);
                    continue;
                }
                _tradingPairs.Add(pair);
            }
        }

        public List<CurrencyPair> TradingPairs { get { return _tradingPairs; }}
        public BinanceSettings BinanceSettings { get {return _binanceSettings;}}
    }

    public struct Authy {
        public readonly string Key;
        public readonly string Secret;

        public Authy(string key, string secret){
            Key = key;
            Secret = secret;
        }
    }

    public struct BinanceSettings {
        public readonly long ReceiveWindow;
        public readonly Authy Credentials;

        public BinanceSettings(Authy authy, long receiveWindow) {
            Credentials = authy;
            ReceiveWindow = receiveWindow;
        }
    }
}