using System;
using System.Collections.Generic;

namespace SpreadShare.Models
{
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
    }
}