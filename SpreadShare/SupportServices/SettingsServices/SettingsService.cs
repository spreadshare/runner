using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
using Dawn;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Poco;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Service for managing settings.
    /// </summary>
    internal class SettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly DatabaseContext _databaseContext;
        private readonly Dictionary<Type, AlgorithmSettings> _algorithmSettingsLookup;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration of the application.</param>
        /// <param name="loggerFactory">LoggerFactory for creating a logger.</param>
        /// <param name="databaseContext">The database context.</param>
        public SettingsService(IConfiguration configuration, ILoggerFactory loggerFactory, DatabaseContext databaseContext)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<SettingsService>();
            _databaseContext = databaseContext;
            _algorithmSettingsLookup = new Dictionary<Type, AlgorithmSettings>();
        }

        /// <summary>
        /// Gets the binance settings.
        /// </summary>
        public Dictionary<Exchange, Dictionary<Type, decimal>> AllocationSettings { get; private set; }

        /// <summary>
        /// Gets the algorithms that are enabled by having allocation.
        /// </summary>
        public List<Type> EnabledAlgorithms => (from e in AllocationSettings
                                                from allocation in e.Value
                                                where allocation.Value > 0
                                                select allocation.Key).ToList();

        /// <summary>
        /// Gets the binance settings.
        /// </summary>
        public BinanceSettings BinanceSettings { get; private set; }

        /// <summary>
        /// Gets the settings for the backtests.
        /// </summary>
        public BacktestSettings BackTestSettings { get; private set; }

        /// <summary>
        /// Gets the administrator settings.
        /// </summary>
        public AdministratorSettings AdministratorSettings { get; private set; }

        private AlgorithmSettings BacktestedAlgorithm =>
            _algorithmSettingsLookup.Values.First(x => x.Exchange == Exchange.Backtesting);

        /// <summary>
        /// Returns the settings instance given a particular Algorithm type.
        /// </summary>
        /// <param name="algo">Type of the algorithm.</param>
        /// <returns>AlgorithmSettings of algo.</returns>
        public AlgorithmSettings GetAlgorithSettings(Type algo)
        {
            Guard.Argument(algo).Require(Reflections.IsAlgorithm);
            return _algorithmSettingsLookup[algo];
        }

        /// <summary>
        /// Starts the settings service.
        /// </summary>
        public void Start()
        {
            try
            {
                // Enables parsing functionality for currencies and should be called first.
                DownloadCurrencies();
                ParseAlgorithmSettings();
                if (Program.CommandLineArgs.Trading)
                {
                    AdministratorSettings = new AdministratorSettings(
                        _configuration
                            .GetSection(nameof(AdministratorSettings))
                            .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true));
                }

                ParseAllocationSettings();
                BinanceSettings = _configuration.GetSection("BinanceClientSettings").Get<BinanceSettings>();
                BackTestSettings = ParseBacktestSettings();
            }
            catch (SocketException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError("Database not available, are you running inside the docker container?");
                Program.ExitProgramWithCode(ExitCode.DatabaseUnreachable);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError($"SettingsService failed to start, aborting other services\n" +
                                 $"Validate that SpreadShare/{Program.CommandLineArgs.ConfigurationPath} is in the correct format.");
                Program.ExitProgramWithCode(ExitCode.InvalidConfiguration);
            }
        }

        /// <summary>
        /// Download all currencies from Binance.
        /// </summary>
        private void DownloadCurrencies()
        {
            using (var client = new BinanceClient())
            {
                // Disect by extracting the known base pairs.
                Regex rx = new Regex(
                    "(.*)(BTC|ETH|USDT|BNB|PAX)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

                var listQuery = client.GetExchangeInfo();
                if (!listQuery.Success)
                {
                    throw new Exception("Could not fetch TradingPair info, no connection to Binance!");
                }

                foreach (var item in listQuery.Data.Symbols)
                {
                    decimal stepSize = 0;
                    decimal pricePrecision = 0;

                    // Extract the pair from the string
                    var pair = rx.Match(item.Name);
                    if (!pair.Success)
                    {
                        _logger.LogWarning($"Could not extract pairs from {item.Name}, skipping");
                        continue;
                    }

                    string left = pair.Groups[1].Value;
                    string right = pair.Groups[2].Value;
                    if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
                    {
                        _logger.LogWarning($"Either left: |{left}| or right: |{right}|  --> was a null or empty string (from {item.Name})");
                        continue;
                    }

                    // Extract the stepSize from the filter
                    foreach (var filter in item.Filters)
                    {
                        if (filter is BinanceSymbolLotSizeFilter filter1)
                        {
                            stepSize = filter1.StepSize;
                        }

                        if (filter is BinanceSymbolPriceFilter filter2)
                        {
                            pricePrecision = filter2.TickSize;
                        }
                    }

                    if (stepSize == 0 || pricePrecision == 0)
                    {
                        _logger.LogWarning($"Could not extract all filters from {item.Name}, skipping");
                        continue;
                    }

                    // Add the instance to the parseTable to make it available for parsing
                    int quantityDecimals = -(int)Math.Log10((double)stepSize);
                    int priceDecimals = -(int)Math.Log10((double)pricePrecision);
                    var result = new TradingPair(new Currency(left), new Currency(right), quantityDecimals, priceDecimals);
                    try
                    {
                        TradingPair.AddParseEntry(pair.Value, result);
                    }
                    catch (ArgumentException)
                    {
                        // Double entries because of binance
                    }
                }
            }
        }

        /// <summary>
        /// Uses reflections to match any class implement BaseStrategy with its AlgorithmSettings.
        /// </summary>
        private void ParseAlgorithmSettings()
        {
            var algoTypes = Reflections.GetAllImplementations(typeof(IBaseAlgorithm)).Where(x => !x.IsAbstract);
            var settingsTypes = Reflections.GetAllSubtypes(typeof(AlgorithmSettings)).ToList();
            foreach (var type in algoTypes)
            {
                string algoName = type.Name;
                _logger.LogInformation($"Matching {algoName} to a {algoName}Settings instance");

                // Filter settings types for current algorithm
                var settingsTypesFiltered = settingsTypes
                    .Where(x => x.Name == $"{algoName}Settings").ToList();

                if (settingsTypesFiltered.Count < 1)
                {
                    throw new InvalidProgramException($"The {algoName}Settings class was not found in the assembly but {algoName} was");
                }

                if (settingsTypesFiltered.Count > 1)
                {
                    throw new InvalidProgramException($"Multiple classes match the filter '{algoName}Settings'\n{settingsTypesFiltered.Join(",\n\t")}");
                }

                var settingsType = settingsTypesFiltered.First();
                var settings = _configuration.GetSection(algoName).Get(settingsType) as AlgorithmSettings;
                if (settings == null)
                {
                    _logger.LogWarning($"{algoName} was not configured in the appsettings.json and will be disabled.");
                    continue;
                }

                // Validate that all extra properties are found in the configuration
                settings.ValidateAllSet(algoName, _configuration);

                // Edge case for parsing [string] -> [TradingPair]
                var currencies = _configuration.GetSection($"{algoName}:ActiveTradingPairs").Get<List<string>>()
                    ?? throw new InvalidDataException($"{algoName}:ActiveTradingPairs could not parsed from json");
                settings.ActiveTradingPairs = currencies.Select(TradingPair.Parse).ToList();

                // Edge case for parsing string -> Currency
                var currencyStr = _configuration.GetSection($"{algoName}:BaseCurrency").Get<string>()
                    ?? throw new InvalidDataException($"{algoName}:BaseCurrency could not be parsed from json");
                settings.BaseCurrency = new Currency(currencyStr);

                // Edge case for parsing string -> Exchange
                var exchangeStr = _configuration.GetSection($"{algoName}:Exchange").Get<string>()
                    ?? throw new InvalidDataException($"{algoName}:Exchange could not be parsed from json");
                settings.Exchange = Enum.Parse<Exchange>(exchangeStr);

                // Validate that the trading pairs are all match with the base currency
                if (settings.ActiveTradingPairs.Any(x => x.Right != settings.BaseCurrency))
                {
                    throw new InvalidDataException($"One or more {algoName}:ActiveTradingPairs was not " +
                                                   $"compatible with the {algoName}:BaseCurrency");
                }

                // Start the backtest communications service before allocation occurs

                // Add settings object to the lookup table
                _algorithmSettingsLookup.Add(type, settings);
            }

            if (_algorithmSettingsLookup.Values.Count(x => x.Exchange == Exchange.Backtesting) > 1)
            {
                throw new InvalidDataException("More than one algorithm was configured for backtesting");
            }
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
            var allocations = _configuration.GetSection("AllocationSettings").GetChildren().ToList();
            if (!allocations.Any())
            {
                throw new Exception("Could not find segment AllocationSettings");
            }

            // Iterate through assembly and classes and retrieve a dictionary with the solution's classes
            var classes = Reflections.GetClasses();

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

                    if (!Reflections.IsAlgorithm(algorithmType))
                    {
                        throw new InvalidConstraintException($"The type {algorithm} does not implement BaseAlgorithm");
                    }

                    var allocation = decimal.Parse(algorithm.Value, NumberStyles.AllowDecimalPoint, new NumberFormatInfo());
                    AllocationSettings[exchangeEnum].Add(algorithmType, allocation);
                }
            }
        }

        private BacktestSettings ParseBacktestSettings()
        {
            BacktestSettings result = new BacktestSettings();
            var rawJson = _configuration.GetSection("BacktestSettings:BacktestPortfolio").GetChildren();
            var parsed = rawJson.ToDictionary(
                x => new Currency(x.Key.ToString(CultureInfo.InvariantCulture)),
                x => new Balance(
                    new Currency(x.Key.ToString(CultureInfo.InvariantCulture)),
                    x.GetValue<decimal>("Free"),
                    x.GetValue<decimal>("Locked")));
            result.InitialPortfolio = new Portfolio(parsed);
            string beginValStr = _configuration.GetSection("BacktestSettings:BeginTimeStamp").Get<string>();
            string endValStr = _configuration.GetSection("BacktestSettings:EndTimeStamp").Get<string>();
            result.OutputFolder = _configuration.GetSection("BacktestSettings:OutputFolder").Get<string>();

            // If no algorithm are configured for backtesting, stop
            if (!_algorithmSettingsLookup.Values.Any(x => x.Exchange == Exchange.Backtesting))
            {
                return result;
            }

            var edges = GetTimeStampEdges();
            result.BeginTimeStamp = beginValStr == "auto" ? edges.Item1 : long.Parse(beginValStr, NumberFormatInfo.InvariantInfo);
            result.EndTimeStamp = endValStr == "auto" ? edges.Item2 : long.Parse(endValStr, NumberFormatInfo.InvariantInfo);

            Guard.Argument(result.BeginTimeStamp).Require(
                x => x % 60000 == 0,
                x => $"BeginTimeStamp must be a multiple of 60000 but is {x}");
            Guard.Argument(result.EndTimeStamp).Require(
                x => x % 60000 == 0,
                x => $"EndTimeStamp must be a multiple of 60000 but is {x}");

            if (result.BeginTimeStamp < edges.Item1)
            {
                _logger.LogError("BeginTimestamp was smaller than one or more of the trading pairs available data");
                throw new ArgumentException("BeginTimestamp");
            }

            if (result.EndTimeStamp > edges.Item2)
            {
                _logger.LogError("EndTimestamp was larger than one or more of the trading pairs available data");
                throw new ArgumentException("EndTimeStamp");
            }

            _logger.LogCritical($"Minimum {result.BeginTimeStamp} ------- Maximum {result.EndTimeStamp}");

            return result;
        }

        private Tuple<long, long> GetTimeStampEdges()
        {
            Guard.Argument(_databaseContext.Candles).NotEmpty(x => $"Database contains no candles!");

            var pairs = BacktestedAlgorithm.ActiveTradingPairs;
            long minBeginVal = 0;
            long minEndVal = long.MaxValue;
            foreach (var pair in pairs)
            {
                if (!_databaseContext.Candles.Where(x => x.TradingPair == pair.ToString()).Any())
                {
                    throw new Exception($"Database does not contain candles for {pair}");
                }

                try
                {
                    long first = _databaseContext.Candles.OrderBy(x => x.Timestamp)
                        .First(x => x.TradingPair == pair.ToString()).Timestamp;
                    if (first > minBeginVal)
                    {
                        minBeginVal = first;
                    }

                    long last = _databaseContext.Candles.OrderBy(x => x.Timestamp)
                        .Last(x => x.TradingPair == pair.ToString()).Timestamp;
                    if (last < minEndVal)
                    {
                        minEndVal = last;
                    }
                }
                catch
                {
                    throw new ArgumentException($"{pair} was not available in the database!");
                }
            }

            return new Tuple<long, long>(minBeginVal, minEndVal);
        }
    }
}