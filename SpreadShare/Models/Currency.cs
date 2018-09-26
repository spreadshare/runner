using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
    static class Currencies
    {

        public static readonly Currency ETH = new Currency("ETH");
        public static readonly Currency BNB = new Currency("BNB");
        public static readonly Currency BTC = new Currency("BTC"); //6 is a guess
    }

    public class Currency
    {
        string _symbol;

        public Currency(string Symbol) {
            _symbol = Symbol;
        }

        public string Symbol { get { return _symbol; }}

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