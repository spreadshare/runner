using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
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
                ReadSimpleBandwagonSettings();
                ReadBinanceSettings();
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
                    decimal stepSize = 0;

                    //Extract the pair from the string
                    var pair = rx.Match(item.Name);
                    if (!pair.Success) {
                            _logger.LogWarning($"Could not extract pairs from {item.Name}, skipping");
                            continue;
                    }
                    string left = pair.Groups[1].Value;
                    string right = pair.Groups[2].Value;


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

        private void ReadSimpleBandwagonSettings() {
            Currency baseCurrency = new Currency(_configuration.GetValue<string>("SimpleBandwagonStrategy:baseCurrency"));
            decimal minimalRevertValue = _configuration.GetValue<decimal>("SimpleBandwagonStrategy:minimalRevertValue");
            decimal minimalGrowthPercentage = _configuration.GetValue<decimal>("SimpleBandwagonStrategy:minimalGrowthPercentage");
            int holdTime = _configuration.GetValue<int>("SimpleBandwagonStrategy:holdTime");
            int checkTime = _configuration.GetValue<int>("SimpleBandwagonStrategy:checkTime");
            SimpleBandWagon = new SimpleBandWagonStrategySettings(baseCurrency, minimalRevertValue, minimalGrowthPercentage, checkTime, holdTime);
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

        public SimpleBandWagonStrategySettings SimpleBandWagon { get; private set; }
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

    internal class SimpleBandWagonStrategySettings {
        public readonly Currency baseCurrency;
        public readonly decimal minimalRevertValue;
        public readonly decimal minimalGrowthPercentage;
        public readonly int checkTime;
        public readonly int holdTime;
        public SimpleBandWagonStrategySettings(Currency baseCurrency, decimal minimalRevertValue, decimal minimalGrowthPercentage,
            int  checkTime, int holdTime)
        {
            this.baseCurrency = baseCurrency;
            this.minimalRevertValue = minimalRevertValue;
            this.minimalGrowthPercentage = minimalGrowthPercentage;
            this.checkTime = checkTime;
            this.holdTime = holdTime;
        }
    }
}