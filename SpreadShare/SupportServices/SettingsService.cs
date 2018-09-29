using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.SupportServices
{
    public class SettingsService : ISettingsService
    {
        IConfiguration _configuration;
        List<CurrencyPair> _activeTradingPairs;
        BinanceSettings _binanceSettings;
        ITradingService _tradingService;
        ILogger _logger;
        
        [Flags]
        public enum Options 
        {
            stepSize = 0
        }

        public SettingsService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _activeTradingPairs = new List<CurrencyPair>();
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

                var listQuery = client.GetExchangeInfo();
                if (!listQuery.Success) {
                    _logger.LogInformation("Could not get exchange info");
                    throw new Exception("No connection to Binance!");
                }
                foreach(var item in listQuery.Data.Symbols) {
                    string left;
                    string right;
                    decimal stepSize = 0;

                    //Extract the pair from the string
                    var pair = rx.Match(item.Name);
                    if (!pair.Success) {
                            _logger.LogWarning($"Could not extract pairs from {item.Name}, skipping");
                            continue;
                    }
                    left = pair.Groups[1].Value;
                    right = pair.Groups[2].Value;


                    //Extract the stepSize from the filter
                    foreach(var filter in item.Filters) {
                        if (filter.FilterType == SymbolFilterType.LotSize) {
                            var nfilter = filter as BinanceSymbolLotSizeFilter;
                            stepSize = nfilter.StepSize;
                        }
                    }
                    if (stepSize == 0) {
                        _logger.LogWarning($"Could not extract stepSize from {item.Name}, skipping");
                        continue;
                    }

                    //Add the instance to the parseTable to make it available for parsing
                    int decimals = -(int)Math.Log10((double)stepSize);
                    var result = new CurrencyPair(new Currency(left), new Currency(right), decimals);
                    CurrencyPair.AddParseEntry(pair.Value, result);
                }
            }
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
                _activeTradingPairs.Add(pair);
            }
        }
        /// <summary>
        /// Get the trading pairs specified in the appsettings
        /// </summary>
        /// <value></value>
        public List<CurrencyPair> ActiveTradingPairs => _activeTradingPairs;
        public BinanceSettings BinanceSettings => _binanceSettings;
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