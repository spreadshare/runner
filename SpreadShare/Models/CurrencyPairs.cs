using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    public class CurrencyPair
    {
        Currency _left;
        Currency _right;
        int _decimals;

        public CurrencyPair(Currency Left, Currency Right, int decimals) {
            if (!(decimals >= 0 && decimals < 10)) throw new ArgumentException("Decimals should be between 0 and 10");
            _left = Left;
            _right = Right;
            _decimals = decimals;
        }

        public Currency Left { get { return _left; }}
        public Currency Right { get { return _right; }}
        public CurrencyPair Flipped { get { return new CurrencyPair(Right, Left, _decimals); }}

        public decimal RoundToTradable(decimal amount) { 
            decimal lotSize = (decimal)Math.Pow(10, _decimals);
            return Math.Floor(amount * lotSize) / lotSize; 
        }

        public override string ToString() {
            return $"{_left}{_right}";
        }
        private static Dictionary<string, CurrencyPair> _table = new Dictionary<string, CurrencyPair>();

        /// <summary>
        /// This function adds a parse option tot the table, this should only be used to initialize the environment
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pair"></param>
        public static void AddParseEntry(string str, CurrencyPair pair) {
            _table.Add(str, pair);
        }

        public static CurrencyPair Parse(string str) {
            if (_table.ContainsKey(str))
                return _table[str];
            throw new Exception($"{str} not found in parse table");
        }
    }
}