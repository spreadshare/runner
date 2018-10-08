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
    /// <summary>
    /// Service for managing settings
    /// </summary>
    internal class SettingsService : ISettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration of the application</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger</param>
        public SettingsService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            ActiveTradingPairs = new List<CurrencyPair>();
            _logger = loggerFactory.CreateLogger<SettingsService>();
        }

        /// <summary>
        /// Gets the list of active trading pairs
        /// </summary>
        public List<CurrencyPair> ActiveTradingPairs { get; }

        /// <summary>
        /// Gets the binance settings
        /// </summary>
        public BinanceSettings BinanceSettings { get; private set; }

        /// <summary>
        /// Gets the settings for the simple bandwagon strategy
        /// </summary>
        public SimpleBandWagonStrategySettings SimpleBandWagonStrategySettings { get; private set; }

        /// <summary>
        /// Gets the (en/dis)able settings of services
        /// </summary>
        public EnabledServices EnabledServices { get; private set; }

        /// <inheritdoc />
        public ResponseObject Start()
        {
            try
            {
                DownloadCurrencies();
                ParseSimpleBandwagonSettings();
                ParseBinanceSettings();
                ParseActiveTradingPairs();
                ParseBinanceSettings();
                ParseEnableServices();
            }
            catch (Exception e)
            {
                return new ResponseObject(ResponseCodes.Error, e.Message);
            }

            return new ResponseObject(ResponseCodes.Success);
        }

        /// <summary>
        /// Download all currencies from Binance
        /// </summary>
        private void DownloadCurrencies()
        {
            using (var client = new BinanceClient())
            {
                // Disect by extracting the known base pairs.
                Regex rx = new Regex(
                    "(.*)(BTC|ETH|USDT|BNB)",
                     RegexOptions.Compiled | RegexOptions.IgnoreCase);

                var listQuery = client.GetExchangeInfo();
                if (!listQuery.Success)
                {
                    _logger.LogInformation("Could not get exchange info");
                    throw new Exception("No connection to Binance!");
                }

                foreach (var item in listQuery.Data.Symbols)
                {
                    decimal stepSize = 0;

                    // Extract the pair from the string
                    var pair = rx.Match(item.Name);
                    if (!pair.Success)
                    {
                            _logger.LogWarning($"Could not extract pairs from {item.Name}, skipping");
                            continue;
                    }

                    string left = pair.Groups[1].Value;
                    string right = pair.Groups[2].Value;

                    // Extract the stepSize from the filter
                    foreach (var filter in item.Filters)
                    {
                        if (filter.FilterType == SymbolFilterType.LotSize)
                        {
                            var nfilter = filter as BinanceSymbolLotSizeFilter;
                            stepSize = nfilter.StepSize;
                        }
                    }

                    if (stepSize == 0)
                    {
                        _logger.LogWarning($"Could not extract stepSize from {item.Name}, skipping");
                        continue;
                    }

                    // Add the instance to the parseTable to make it available for parsing
                    int decimals = -(int)Math.Log10((double)stepSize);
                    var result = new CurrencyPair(new Currency(left), new Currency(right), decimals);
                    CurrencyPair.AddParseEntry(pair.Value, result);
                }
            }
        }

        /// <summary>
        /// Parse settings for the simple bandwagon strategy
        /// </summary>
        private void ParseSimpleBandwagonSettings()
        {
            Currency baseCurrency = new Currency(_configuration.GetValue<string>("SimpleBandwagonStrategy:baseCurrency"));
            decimal minimalRevertValue = _configuration.GetValue<decimal>("SimpleBandwagonStrategy:minimalRevertValue");
            decimal minimalGrowthPercentage = _configuration.GetValue<decimal>("SimpleBandwagonStrategy:minimalGrowthPercentage");
            int holdTime = _configuration.GetValue<int>("SimpleBandwagonStrategy:holdTime");
            int checkTime = _configuration.GetValue<int>("SimpleBandwagonStrategy:checkTime");
            SimpleBandWagonStrategySettings = new SimpleBandWagonStrategySettings(baseCurrency, minimalRevertValue, minimalGrowthPercentage, checkTime, holdTime);
        }

        /// <summary>
        /// Parse settings for Binance
        /// </summary>
        private void ParseBinanceSettings()
        {
            string key = _configuration.GetValue<string>("BinanceCredentials:api-key");
            string secret = _configuration.GetValue<string>("BinanceCredentials:api-secret");
            Authy authy = new Authy(key, secret);

            long receiveWindow = _configuration.GetValue<long>("BinanceClientSettings:receiveWindow");
            BinanceSettings = new BinanceSettings(authy, receiveWindow);
        }

        /// <summary>
        /// Parse the active trading pairs
        /// </summary>
        private void ParseActiveTradingPairs()
        {
            var tradingPairs = _configuration.GetSection("BinanceClientSettings:tradingPairs").AsEnumerable().ToArray();
            foreach (var tradingPair in tradingPairs)
            {
                CurrencyPair pair;
                try
                {
                    pair = CurrencyPair.Parse(tradingPair.Value);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                    continue;
                }

                ActiveTradingPairs.Add(pair);
            }
        }

        /// <summary>
        /// Parse the enabled services
        /// </summary>
        private void ParseEnableServices()
        {
            bool strategyServiceEnabled = _configuration.GetValue<bool>("EnableServices:strategy");
            bool tradingServiceEnabled = _configuration.GetValue<bool>("EnableServices:trading");
            bool userServiceEnabled = _configuration.GetValue<bool>("EnableServices:user");
            bool zeroMqServiceEnabled = _configuration.GetValue<bool>("EnableServices:zeroMq");

            EnabledServices = new EnabledServices(
                strategyServiceEnabled,
                tradingServiceEnabled,
                userServiceEnabled,
                zeroMqServiceEnabled);
        }
    }
}