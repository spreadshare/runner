using System;
using System.Collections.Generic;
using System.Linq;

namespace SpreadShare.Models
{
    /// <summary>
    /// Represents the portfolio of an exchange wallet.
    /// </summary>
    internal class Assets
    {
        private readonly Dictionary<Currency, decimal> _free;
        private readonly Dictionary<Currency, decimal> _locked;
        private readonly Dictionary<Currency, decimal> _total;

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets"/> class.
        /// </summary>
        public Assets()
        {
            _free = new Dictionary<Currency, decimal>();
            _locked = new Dictionary<Currency, decimal>();
            _total = new Dictionary<Currency, decimal>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets"/> class.
        /// </summary>
        /// <param name="input">List of balances</param>
        public Assets(List<ExchangeBalance> input)
            : this()
        {
            foreach (var balance in input)
            {
                _free.Add(new Currency(balance.Symbol), balance.Free);
                _locked.Add(new Currency(balance.Symbol), balance.Locked);
                _total.Add(new Currency(balance.Symbol), balance.Total);
            }
        }

        /// <summary>
        /// Gets the balance of a currency that is unallocated
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The unallocated balance of a currency</returns>
        public decimal GetFreeBalance(Currency symbol)
        {
            return _free.GetValueOrDefault(symbol, 0);
        }

        /// <summary>
        /// Gets the balance of a currency that is locked
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The locked balance of a currency</returns>
        public decimal GetLockedBalance(Currency symbol)
        {
            return _locked.GetValueOrDefault(symbol, 0);
        }

        /// <summary>
        /// Gets the total balance of a currency
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The total balance of a currency</returns>
        public decimal GetTotalBalance(Currency symbol)
        {
            return _free.GetValueOrDefault(symbol, 0);
        }

        /// <summary>
        /// Gets all non-zero balances that are unallocated
        /// </summary>
        /// <returns>All non-zero balances that are unallocated</returns>
        public List<AssetValue> GetAllFreeBalances()
        {
            return (from symbol in _free.Keys where _free[symbol] > 0 select new AssetValue(symbol, _free[symbol])).ToList();
        }

        /// <summary>
        /// Gets all non-zero balances that are locked
        /// </summary>
        /// <returns>All non-zero balances that are locked</returns>
        public List<AssetValue> GetAllLockedBalances()
        {
            return (from symbol in _locked.Keys where _locked[symbol] > 0 select new AssetValue(symbol, _locked[symbol])).ToList();
        }

        /// <summary>
        /// Gets all non-zero balances
        /// </summary>
        /// <returns>All non-zero balances</returns>
        public List<AssetValue> GetAllTotalBalances()
        {
            return (from symbol in _total.Keys where _total[symbol] > 0 select new AssetValue(symbol, _total[symbol])).ToList();
        }

        /// <summary>
        /// Gets a copy of the Assets scaled down with given scale
        /// </summary>
        /// <param name="scale">Decimal between 0 and 1 indicating the scale</param>
        /// <returns>Exchange balance corresponding to the given currency</returns>
        public Assets DuplicateWithScale(decimal scale)
        {
            if (scale < 0 || scale > 1)
            {
                throw new ArgumentException("Argument 'scale' should be between 0 and 1.");
            }

            return new Assets(GetAllTotalBalances()
                .Select(assetValue => new ExchangeBalance(
                    assetValue.Symbol.ToString(),
                    GetFreeBalance(assetValue.Symbol) * scale,
                    GetLockedBalance(assetValue.Symbol) * scale))
                .ToList());
        }

        private List<ExchangeBalance> GetExchangeBalances()
        {
            List<ExchangeBalance> balances = new List<ExchangeBalance>();
            
            foreach (var balance in GetAllTotalBalances())
            {
                balances.Add(
                    new ExchangeBalance(
                        balance.Symbol.ToString(),
                        GetFreeBalance(balance.Symbol),
                        GetLockedBalance(balance.Symbol)
                    ));
            }

            return balances;
        }
        
        public Assets Combine(Assets other)
        {
            List<ExchangeBalance> result = new List<ExchangeBalance>();
            var balancesThis = this.GetExchangeBalances();

            // Result += contains all Other.Currencies
            foreach (var balance in other.GetExchangeBalances())
            {
                // Get [Symbol, Free, Locked] from current balances
                ExchangeBalance temp = balancesThis.SingleOrDefault(b => b.Symbol.Equals(balance.Symbol));
                if (temp == null)
                {
                    temp = new ExchangeBalance(balance.Symbol, 0.0M, 0.0M);
                }
                
                result.Add(new ExchangeBalance(
                    balance.Symbol,
                    temp.Free + balance.Free,
                    temp.Locked + balance.Locked
                    ));                
            }
            
            // Result += Where (this.Currency NOT IN other.Currency)
            foreach (var balance in balancesThis)
            {
                if (result.Select(x => x.Symbol).Contains(balance.Symbol))
                {
                    continue;
                }
                result.Add(new ExchangeBalance(balance.Symbol, balance.Free, balance.Locked));
            }

            return new Assets(result);
        }
        
        public Assets Intersection(Assets other)
        {
            throw new NotImplementedException();
        }
    }
}