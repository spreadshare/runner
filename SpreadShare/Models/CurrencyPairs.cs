using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    static class CurrencyPairs
    {
        public static readonly CurrencyPair BNBETH = new CurrencyPair(Currencies.BNB, Currencies.ETH, 2);
        public static readonly CurrencyPair ETHBTC = new CurrencyPair(Currencies.ETH, Currencies.BTC, 2);
        public static readonly CurrencyPair BNBBTC = new CurrencyPair(Currencies.BNB, Currencies.BTC, 2);
    }

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
            decimal lotSize = 10^_decimals;
            return Math.Floor(amount * lotSize) * lotSize; 
        }

        public override string ToString() {
            return $"{_left}{_right}";
        }

        private static Dictionary<string, CurrencyPair> _table = new Dictionary<string, CurrencyPair> {
            { "BNBETH", CurrencyPairs.BNBETH },
            { "ETCBTC", CurrencyPairs.ETHBTC },
            { "BNBBTC", CurrencyPairs.BNBBTC }
        };

        public static CurrencyPair Parse(string str) {
            if (_table.ContainsKey(str))
                return _table[str];
            throw new Exception($"{str} not found in parse table");
        }
    }
}