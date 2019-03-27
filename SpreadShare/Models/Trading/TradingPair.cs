using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Binance.Net;
using Binance.Net.Objects;
using Dawn;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a trading pair.
    /// </summary>
    [JsonConverter(typeof(TradingPairSerializer))]
    internal class TradingPair
    {
        private static readonly Dictionary<string, TradingPair> Table = new Dictionary<string, TradingPair>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingPair"/> class.
        /// </summary>
        /// <param name="left">Left side of the trading pair.</param>
        /// <param name="right">Right side of the trading pair.</param>
        /// <param name="quantityDecimals">Number of decimals the TradingPairs' volume can be expressed in.</param>
        /// <param name="priceDecimals">Number of decimals the TradingPairs' price can be expressed in. </param>
        public TradingPair(Currency left, Currency right, int quantityDecimals, int priceDecimals)
        {
            Guard.Argument(left).NotNull().NotEqual(Guard.Argument(right).NotNull());
            Guard.Argument(quantityDecimals).NotNegative();
            Guard.Argument(priceDecimals).NotNegative();

            Left = left;
            Right = right;
            QuantityDecimals = quantityDecimals;
            PriceDecimals = priceDecimals;
        }

        /// <summary>
        /// Gets the left side of the trading pair.
        /// </summary>
        public Currency Left { get; }

        /// <summary>
        /// Gets the right side of the trading pair.
        /// </summary>
        public Currency Right { get; }

        /// <summary>
        /// Gets the number of quantityDecimals.
        /// </summary>
        public int QuantityDecimals { get; }

        /// <summary>
        /// Gets the number of priceDecimals.
        /// </summary>
        public int PriceDecimals { get; }

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment.
        /// </summary>
        /// <param name="tradingPairString">String representation of the trading pair.</param>
        /// <param name="tradingPair">The trading pair.</param>
        public static void AddParseEntry(string tradingPairString, TradingPair tradingPair)
        {
            Guard.Argument(tradingPairString).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(tradingPair).NotNull();

            // Clean the pair string of all whitespace
            string cleanedPairString = RemoveAllWhiteSpace(tradingPairString);

            if (Table.ContainsKey(cleanedPairString))
            {
                Table[cleanedPairString] = tradingPair;
                return;
            }

            Table.Add(cleanedPairString, tradingPair);
        }

        /// <summary>
        /// Parse given string to trading pair.
        /// </summary>
        /// <param name="tradingPairString">String representation of tradingPair.</param>
        /// <returns>The trading pair matching the string.</returns>
        public static TradingPair Parse(string tradingPairString)
        {
            Guard.Argument(tradingPairString).NotNull().NotEmpty().NotWhiteSpace();

            // Clean the pair string of all whitespace
            string cleanedPairString = RemoveAllWhiteSpace(tradingPairString);

            if (Table.ContainsKey(cleanedPairString))
            {
                return Table[cleanedPairString];
            }

            throw new KeyNotFoundException($"{cleanedPairString} not found in parse table");
        }

        /// <summary>
        /// Parse given two currencies.
        /// </summary>
        /// <param name="left">Left currency.</param>
        /// <param name="right">Right currency.</param>
        /// <returns>Parsed trading pair.</returns>
        public static TradingPair Parse(Currency left, Currency right)
        {
            Guard.Argument(left).NotNull();
            Guard.Argument(right).NotNull();
            string str = $"{left}{right}";

            if (Table.ContainsKey(str))
            {
                return Table[str];
            }

            throw new KeyNotFoundException($"{str} not found in parse table");
        }

        /// <summary>
        /// Download all currencies from Binance.
        /// </summary>
        /// <param name="logger">Used to create output.</param>
        public static void Sync(ILogger logger)
        {
            using (var client = new BinanceClient())
            {
                // Disect by extracting the known base pairs.
                Regex rx = new Regex(
                    "(.*)(BNB|BTC|ETH|XRP|USD|USDT|TUSD|PAX|USDC|USDS)",
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
                        logger.LogWarning($"Could not extract pairs from {item.Name}, skipping");
                        continue;
                    }

                    string left = pair.Groups[1].Value;
                    string right = pair.Groups[2].Value;
                    if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
                    {
                        logger.LogWarning($"Either left: |{left}| or right: |{right}|  --> was a null or empty string (from {item.Name})");
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
                        logger.LogWarning($"Could not extract all filters from {item.Name}, skipping");
                        continue;
                    }

                    // Add the instance to the parseTable to make it available for parsing
                    int quantityDecimals = -(int)Math.Log10((double)stepSize);
                    int priceDecimals = -(int)Math.Log10((double)pricePrecision);
                    var result = new TradingPair(new Currency(left), new Currency(right), quantityDecimals, priceDecimals);
                    try
                    {
                        AddParseEntry(pair.Value, result);
                    }
                    catch (ArgumentException)
                    {
                        // Double entries because of binance
                    }
                }
            }
        }

        /// <summary>
        /// Round unrounded quantity to the tradable quantity conform to the TradingPairs' specification.
        /// </summary>
        /// <param name="quantity">Unrounded quantity.</param>
        /// <returns>Rounded quantity.</returns>
        public decimal RoundToTradable(decimal quantity)
        {
            Guard.Argument(quantity).NotNegative(x => $"Quantity is negative namely: {x}");
            decimal value = Math.Round(quantity, QuantityDecimals);
            return value <= quantity ? value : value - (decimal)Math.Pow(10, -QuantityDecimals);
        }

        /// <summary>
        /// Round unrounded price to a priceable amount conform the the TradingPairs' specification.
        /// </summary>
        /// <param name="price">Unrounded price.</param>
        /// <returns>Rounded price.</returns>
        public decimal RoundToPriceable(decimal price)
        {
            Guard.Argument(price).NotNegative(x => $"Price is negative name: {x}");
            decimal value = Math.Round(price, PriceDecimals);
            return value <= price ? value : value - (decimal)Math.Pow(10, -PriceDecimals);
        }

        /// <summary>
        /// Round unrounded balance to tradable quantity conform to the TradingPair's specification.
        /// </summary>
        /// <param name="balance">Balance to round.</param>
        /// <returns>Rounded Balance.</returns>
        public Balance RoundToTradable(Balance balance)
        {
            return new Balance(
                balance.Symbol,
                RoundToTradable(balance.Free),
                RoundToTradable(balance.Locked));
        }

        /// <summary>
        /// Returns a string representation of the trading pair.
        /// </summary>
        /// <returns>A string representation of the trading pair.</returns>
        public override string ToString()
        {
            return $"{Left}{Right}";
        }

        private static string RemoveAllWhiteSpace(string input)
        {
            return new string(input.Where(x => !char.IsWhiteSpace(x)).ToArray());
        }
    }

    /// <summary>
    /// Serialize/Deserialize TradingPair as string.
    /// </summary>
    internal class TradingPairSerializer : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var pair = value as TradingPair ?? throw new NullReferenceException("Cannot parse null to TradingPair");
            serializer.Serialize(writer, pair.ToString());
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = serializer.Deserialize<string>(reader);
            return TradingPair.Parse(str);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TradingPair);
        }
    }
}

#pragma warning restore SA1402