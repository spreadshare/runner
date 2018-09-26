using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    static class Currencies
    {

        public static Currency ETH = new Currency("ETH", 2);
        public static Currency BNB = new Currency("BNB", 2);
    }

    public class Currency
    {
        string _symbol;
        decimal _lotSize;

        public Currency(string Symbol, int decimals) {
            if (!(decimals >= 0 && decimals < 10)) throw new ArgumentException("Decimals should be between 0 and 10");
            _symbol = Symbol;
            _lotSize = 10^decimals;
        }

        public string Symbol { get { return _symbol; }}
        public decimal RoundToTradable(decimal amount) { return Math.Floor(amount * _lotSize) * _lotSize; }

        public override string ToString() {
            return Symbol;
        }
        
        
        
        private static Dictionary<string, Currency> _table = new Dictionary<string, Currency>()
        {
            { "ETH", Currencies.ETH },
            { "BNB", Currencies.BNB }
        };

        public static Currency Parse(string str)
        {
            if (_table.ContainsKey(str)) {
                return _table[str];
            } else {
                throw new Exception($"Currency {str} was not defined to parse");
            }
        }
    }
}