using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.Models;

namespace SpreadShare.SupportServices.SettingsServices
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
            _logger = loggerFactory.CreateLogger<SettingsService>();
        }

        /// <summary>
        /// Gets the binance settings
        /// </summary>
        public BinanceSettings BinanceSettings { get; private set; }

        /// <summary>
        /// Gets the settings for the simple bandwagon algorithm
        /// </summary>
        public SimpleBandWagonAlgorithmSettings SimpleBandWagonAlgorithmSettings { get; private set; }

        /// <summary>
        /// Gets the (en/dis)able settings of services
        /// </summary>
        public EnabledServices EnabledServices { get; private set; }

        /// <inheritdoc />
        public ResponseObject Start()
        {
            try
            {
                // Enables parsing functionality for currencies and should be called first.
                DownloadCurrencies();
                ParseSimpleBandwagonSettings();
                BinanceSettings = _configuration.GetSection("BinanceClientSettings").Get<BinanceSettings>();
                EnabledServices = _configuration.GetSection("EnabledServices").Get<EnabledServices>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new ResponseObject(ResponseCode.Error, e.Message);
            }

            return new ResponseObject(ResponseCode.Success);
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
        /// Parse settings for the simple bandwagon algorithm.
        /// </summary>
        private void ParseSimpleBandwagonSettings()
        {
            SimpleBandWagonAlgorithmSettings = _configuration.GetSection("SimpleBandWagonAlgorithm").Get<SimpleBandWagonAlgorithmSettings>();

            // Get the ActiveTradingPairs as a seperate string list
            var currencies = _configuration.GetSection("SimpleBandWagonAlgorithm:ActiveTradingPairs")
                .Get<List<string>>();

            // Map the trading pairs to currencies by parsing and assign to the settings.
            SimpleBandWagonAlgorithmSettings.ActiveTradingPairs = currencies.Select(CurrencyPair.Parse).ToList();

            // Parse the base currency string to a Currency type
            var baseStr = _configuration.GetSection("SimpleBandWagonAlgorithm:BaseCurrency").Get<string>();
            SimpleBandWagonAlgorithmSettings.BaseCurrency = new Currency(baseStr);

            // TODO: Parse exchange in the AlgorithmSettings class

            // Parse exchange to enum Exchange
            var exchange = _configuration.GetSection("SimpleBandWagonAlgorithm:Exchange").Get<string>();
            //SimpleBandWagonAlgorithmSettings.Exchange = Enum.Parse<Exchange>(exchange);
            SimpleBandWagonAlgorithmSettings.Exchange = Exchange.Binance;
        }
    }
}