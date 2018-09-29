using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    public class CurrencyPair
    {
        private static readonly Dictionary<string, CurrencyPair> Table = new Dictionary<string, CurrencyPair>();
        private readonly int _decimals;

        public Currency Left { get; }
        public Currency Right { get; }
        public CurrencyPair Flipped => new CurrencyPair(Right, Left, _decimals);


        public CurrencyPair(Currency left, Currency right, int decimals) {
            if (!(decimals >= 0 && decimals < 10)) throw new ArgumentException("Decimals should be between 0 and 10");
            Left = left;
            Right = right;
            _decimals = decimals;
        }
        
        /// <summary>
        /// Round unrounded amount to the tradable amount conform to currency's decimals
        /// </summary>
        /// <param name="amount">Unrounded amount</param>
        /// <returns>Rounded amount</returns>
        public decimal RoundToTradable(decimal amount) { 
            decimal lotSize = (decimal)Math.Pow(10, _decimals);
            return Math.Floor(amount * lotSize) / lotSize; 
        }

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pair"></param>
        public static void AddParseEntry(string str, CurrencyPair pair) {
            Table.Add(str, pair);
        }

        /// <summary>
        /// Parse given string to currency pair
        /// </summary>
        /// <param name="currencyPair">String representation of currencyPair</param>
        /// <returns>CurrencyPair</returns>
        public static CurrencyPair Parse(string currencyPair) {
            if (Table.ContainsKey(currencyPair))
                return Table[currencyPair];
            throw new Exception($"{currencyPair} not found in parse table");
        }

        public override string ToString()
        {
            return $"{Left}{Right}";
        }
    }
}