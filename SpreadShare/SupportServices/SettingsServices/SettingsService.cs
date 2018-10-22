using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
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
        public Dictionary<Exchange, Dictionary<Type, decimal>> AllocationSettings { get; private set; }

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
                ParseAllocationSettings();
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
                    try
                    {
                        CurrencyPair.AddParseEntry(pair.Value, result);
                    }
                    catch (ArgumentException ignored)
                    {
                        // Double entries because of binance
                    }
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

        /// <summary>
        /// Parse allocations settings by evaluating each child as algorithm.
        /// Note that duplicate entries are automatically removed by IConfiguration.
        /// Only the last entry remains.
        /// </summary>
        private void ParseAllocationSettings()
        {
            AllocationSettings = new Dictionary<Exchange, Dictionary<Type, decimal>>();

            // Get configuration section
            var allocations = _configuration.GetSection("AllocationSettings").GetChildren();
            if (!allocations.Any())
            {
                throw new Exception("Could not find segment AllocationSettings");
            }

            // Iterate through assembly and classes and retrieve a dictionary with the solution's classes
            var classes = GetClasses();

            // Iterate over exchanges
            foreach (var exchange in allocations)
            {
                // Enum not found --> Exception
                var exchangeEnum = Enum.Parse<Exchange>(exchange.Key);
                AllocationSettings.Add(exchangeEnum, new Dictionary<Type, decimal>());

                // Iterate over algorithms
                foreach (var algorithm in exchange.GetChildren())
                {
                    Type algorithmType = classes[algorithm.Key].AsType();

                    if (algorithmType == null)
                    {
                        throw new InvalidConstraintException($"Could not parse algorithm {algorithm.Key}");
                    }

                    if (algorithmType.BaseType != typeof(BaseAlgorithm))
                    {
                        throw new InvalidConstraintException($"The type {algorithm} does not implement BaseAlgorithm");
                    }

                    var allocation = decimal.Parse(algorithm.Value, NumberStyles.AllowDecimalPoint, new NumberFormatInfo());
                    AllocationSettings[exchangeEnum].Add(algorithmType, allocation);
                }
            }
        }

        /// <summary>
        /// Get a dictionary of classes with class name
        /// </summary>
        /// <returns>Dictionary of classes with class name</returns>
        private Dictionary<string, TypeInfo> GetClasses()
        {
            // Get current assembly
            Assembly thisAssembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), assembly
                => assembly.ManifestModule.Name.Contains("SpreadShare.dll", StringComparison.InvariantCulture));

            // Check if the assembly was found
            if (thisAssembly == null)
            {
                throw new InvalidProgramException("Could not find SpreadShare.dll");
            }

            // Get all defined classes
            var typeInfos = thisAssembly.DefinedTypes;
            Dictionary<string, TypeInfo> classes = new Dictionary<string, TypeInfo>();
            foreach (var typeInfo in typeInfos)
            {
                var typeInfoName = typeInfo.Name;

                // Remove noise
                string[] patterns = { "`1", "+<>c" };
                foreach (string pattern in patterns)
                {
                    if (typeInfoName.EndsWith(pattern, true, CultureInfo.InvariantCulture))
                    {
                        typeInfoName = typeInfoName.Substring(0, typeInfoName.Length - pattern.Length);
                    }
                }

                if (!classes.ContainsKey(typeInfoName))
                {
                    classes.Add(typeInfoName, typeInfo);
                }
                else
                {
                    // Make sure the algorithm class does not have different parameterized types
                    classes[typeInfoName] = null;
                }
            }

            return classes;
        }
    }
}