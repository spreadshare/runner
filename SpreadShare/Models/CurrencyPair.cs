using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    /// <summary>
    /// Object representation of a currency pair
    /// </summary>
    /// TODO: Should this not be called trading pair?
    public class CurrencyPair
    {
        private static readonly Dictionary<string, CurrencyPair> Table = new Dictionary<string, CurrencyPair>();
        private readonly int _decimals;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyPair"/> class.
        /// </summary>
        /// <param name="left">Left side of the currency pair</param>
        /// <param name="right">Right side of the currency pair</param>
        /// <param name="decimals">Amount of decimals the currency pair can be expressed in</param>
        public CurrencyPair(Currency left, Currency right, int decimals)
        {
            if (!(decimals >= 0 && decimals < 10))
            {
                throw new ArgumentException("Decimals should be between 0 and 10");
            }

            Left = left;
            Right = right;
            _decimals = decimals;
        }

        /// <summary>
        /// Gets the left side of the currency pair
        /// </summary>
        public Currency Left { get; }

        /// <summary>
        /// Gets the right side of the currency pair
        /// </summary>
        public Currency Right { get; }

        /// <summary>
        /// Gets a flipped version of the currency pair
        /// </summary>
        public CurrencyPair Flipped => new CurrencyPair(Right, Left, _decimals);

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment
        /// </summary>
        /// <param name="stringCurrencyPair">String representation of the currency pair</param>
        /// <param name="currencyPair">The currency pair</param>
        public static void AddParseEntry(string stringCurrencyPair, CurrencyPair currencyPair)
        {
            Table.Add(stringCurrencyPair, currencyPair);
        }

        /// <summary>
        /// Parse given string to currency pair
        /// </summary>
        /// <param name="currencyPair">String representation of currencyPair</param>
        /// <returns>The currency pair matching the string</returns>
        public static CurrencyPair Parse(string currencyPair)
        {
            if (Table.ContainsKey(currencyPair))
            {
                return Table[currencyPair];
            }

            // TODO: Is this developer fault or input error? We should not throw exceptions on input errors
            throw new Exception($"{currencyPair} not found in parse table");
        }

        /// <summary>
        /// Round unrounded amount to the tradable amount conform to currency's decimals
        /// </summary>
        /// <param name="amount">Unrounded amount</param>
        /// <returns>Rounded amount</returns>
        public decimal RoundToTradable(decimal amount)
        {
            decimal lotSize = (decimal)Math.Pow(10, _decimals);
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
    }
}