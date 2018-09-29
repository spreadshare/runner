using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    internal class SettingsService : ISettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SettingsService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            ActiveTradingPairs = new List<CurrencyPair>();
            _logger = loggerFactory.CreateLogger<SettingsService>();
        }
        public ResponseObject Start()
        {
            try {
                DownloadCurrencies();
                ReadTradingPairs();
                ReadBinanceSettings();
            } catch(Exception e) {
                return new ResponseObject(ResponseCodes.Error, e.Message);
            }
            return new ResponseObject(ResponseCodes.Success);
        }

        private void DownloadCurrencies() {
            using(var client = new BinanceClient())
            {
                //Disect by extracting the known base pairs.
                Regex rx = new Regex("(.*)(BTC|ETH|USDT|BNB)",
                     RegexOptions.Compiled | RegexOptions.IgnoreCase);


                var listQuery = client.GetAllPrices();
                if (listQuery.Success) {
                    foreach(var item in listQuery.Data) {
                        var pairs = rx.Matches(item.Symbol);
                        foreach(var pair in pairs.Reverse()) {
                            if (!pair.Success) {
                                _logger.LogWarning($"Could not extract pairs from {item.Symbol}");
                                continue;
                            }
                            string left = pair.Groups[1].Value;
                            string right = pair.Groups[2].Value;
                            var result = new CurrencyPair(new Currency(left), new Currency(right), 2);
                            //Add the instance to the parseTable to make it available for parsing
                            CurrencyPair.AddParseEntry(pair.Value, result);
                        }
                    }
                }
            }
        }

        private void ReadBinanceSettings() {
            string key = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string secret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            Authy authy = new Authy(key, secret);

            long receiveWindow = _configuration.GetValue<long>("BinanceClientSettings:receiveWindow");
            BinanceSettings = new BinanceSettings(authy, receiveWindow);

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
                ActiveTradingPairs.Add(pair);
            }
        }
        /// <summary>
        /// Get the trading pairs specified in the appsettings
        /// </summary>
        /// <value></value>
        public List<CurrencyPair> ActiveTradingPairs { get; }
        public BinanceSettings BinanceSettings { get; private set; }
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