using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Object representation of a currency pair
    /// </summary>
    internal class TradingPair
    {
        private static readonly Dictionary<string, TradingPair> Table = new Dictionary<string, TradingPair>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingPair"/> class.
        /// </summary>
        /// <param name="left">Left side of the currency pair</param>
        /// <param name="right">Right side of the currency pair</param>
        /// <param name="decimals">Amount of decimals the currency pair can be expressed in</param>
        public TradingPair(Currency left, Currency right, int decimals)
        {
            if (!(decimals >= 0))
            {
                throw new ArgumentException("Decimals should be larger than 0");
            }

            Left = left;
            Right = right;
            Decimals = decimals;
        }

        /// <summary>
        /// Gets the left side of the currency pair
        /// </summary>
        public Currency Left { get; }

        /// <summary>
        /// Gets the right side of the currency pair
        /// </summary>
        public Currency Right { get; }
        
        public int Decimals { get; }

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment
        /// </summary>
        /// <param name="tradingPairString">String representation of the currency pair</param>
        /// <param name="tradingPair">The currency pair</param>
        public static void AddParseEntry(string tradingPairString, TradingPair tradingPair)
        {
            // Clean the pair string of all whitespace
            string cleanedPairString = RemoveAllWhiteSpace(tradingPairString);

            Table.Add(tradingPairString, tradingPair);
        }

        /// <summary>
        /// Parse given string to currency pair
        /// </summary>
        /// <param name="tradingPairString">String representation of tradingPair</param>
        /// <returns>The currency pair matching the string</returns>
        public static TradingPair Parse(string tradingPairString)
        {
            if (string.IsNullOrWhiteSpace(tradingPairString))
            {
                throw new ArgumentException("Trading pair string must not be null or whitespace");
            }

            // Clean the pair string of all whitespace
            string cleanedPairString = RemoveAllWhiteSpace(tradingPairString);

            if (Table.ContainsKey(cleanedPairString))
            {
                return Table[cleanedPairString];
            }

            throw new KeyNotFoundException($"{cleanedPairString} not found in parse table");
        }

        /// <summary>
        /// Round unrounded amount to the tradable amount conform to currency's decimals
        /// </summary>
        /// <param name="amount">Unrounded amount</param>
        /// <returns>Rounded amount</returns>
        public decimal RoundToTradable(decimal amount)
        {
            long lotSize = (long)Math.Pow(10, Decimals);
            return Math.Floor(amount * lotSize) / lotSize;
        }

        /// <summary>
        /// Returns a string representation of the currency pair
        /// </summary>
        /// <returns>A string representation of the currency pair</returns>
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
            while ( pow != 0 )
            {
                if ( (pow & 1) == 1 )
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