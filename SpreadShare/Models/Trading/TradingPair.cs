using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a trading pair
    /// </summary>
    internal class TradingPair
    {
        private static readonly Dictionary<string, TradingPair> Table = new Dictionary<string, TradingPair>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingPair"/> class.
        /// </summary>
        /// <param name="left">Left side of the trading pair</param>
        /// <param name="right">Right side of the trading pair</param>
        /// <param name="decimals">Number of decimals the trading pair can be expressed in</param>
        public TradingPair(Currency left, Currency right, int decimals)
        {
            Guard.Argument(left).NotNull().NotEqual(Guard.Argument(right).NotNull());
            Guard.Argument(decimals).NotNegative();

            Left = left;
            Right = right;
            Decimals = decimals;
        }

        /// <summary>
        /// Gets the left side of the trading pair
        /// </summary>
        public Currency Left { get; }

        /// <summary>
        /// Gets the right side of the trading pair
        /// </summary>
        public Currency Right { get; }

        /// <summary>
        /// Gets the number of decimals
        /// </summary>
        public int Decimals { get; }

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment
        /// </summary>
        /// <param name="tradingPairString">String representation of the trading pair</param>
        /// <param name="tradingPair">The trading pair</param>
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
        /// Parse given string to trading pair
        /// </summary>
        /// <param name="tradingPairString">String representation of tradingPair</param>
        /// <returns>The trading pair matching the string</returns>
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
        /// Parse given two currencies
        /// </summary>
        /// <param name="left">left currency</param>
        /// <param name="right">right currency</param>
        /// <returns>Parsed trading pair</returns>
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
        /// Round unrounded amount to the tradable amount conform to TradingPair's decimals
        /// </summary>
        /// <param name="quantity">Unrounded quantity</param>
        /// <returns>Rounded amount</returns>
        public decimal RoundToTradable(decimal quantity)
        {
            long lotSize = IntPow(10, (uint)Decimals);
            return Math.Floor(quantity * lotSize) / lotSize;
        }

        /// <summary>
        /// Returns a string representation of the trading pair
        /// </summary>
        /// <returns>A string representation of the trading pair</returns>
        public override string ToString()
        {
            return $"{Left}{Right}";
        }

        private static string RemoveAllWhiteSpace(string input)
        {
            return new string(input.Where(x => !char.IsWhiteSpace(x)).ToArray());
        }

        private static int IntPow(int x, uint pow)
        {
            int ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                {
                    ret *= x;
                }

                x *= x;
                pow >>= 1;
            }

            return ret;
        }
    }
}