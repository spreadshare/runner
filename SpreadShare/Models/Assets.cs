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
                _free.Add(balance.Symbol, balance.Free);
                _locked.Add(balance.Symbol, balance.Locked);
                _total.Add(balance.Symbol, balance.Total);
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
                    assetValue.Symbol,
                    GetFreeBalance(assetValue.Symbol) * scale,
                    GetLockedBalance(assetValue.Symbol) * scale))
                .ToList());
        }

        /// <summary>
        /// Combine two asset collections and return the result
        /// </summary>
        /// <param name="other">The other instance to combine with</param>
        /// <returns>Combined result</returns>
        public Assets Union(Assets other)
        {
            if (other == null)
            {
                return this;
            }

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
                    temp.Locked + balance.Locked));
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

        private List<ExchangeBalance> GetExchangeBalances()
        {
            List<ExchangeBalance> balances = new List<ExchangeBalance>();

            foreach (var balance in GetAllTotalBalances())
            {
                balances.Add(
                    new ExchangeBalance(
                        balance.Symbol,
                        GetFreeBalance(balance.Symbol),
                        GetLockedBalance(balance.Symbol)));
            }

            return balances;
        }

        /// <summary>
        /// Get difference between this and other (not in other)
        /// </summary>
        /// <param name="other">The other asset collection</param>
        /// <returns>All assets unique to this assets object</returns>
        public Assets Difference(Assets other)
        {
            if (other == null)
            {
                return this;
            }

            var freeLeft = new List<AssetValue>();
            foreach (var assetValue in GetAllFreeBalances())
            {
                var diff = assetValue.Amount - other.GetFreeBalance(assetValue.Symbol);
                if (diff > 0)
                {
                    freeLeft.Add(new AssetValue(assetValue.Symbol, diff));
                }
            }

            var lockedLeft = new List<AssetValue>();
            foreach (var assetValue in GetAllLockedBalances())
            {
                var diff = assetValue.Amount - other.GetLockedBalance(assetValue.Symbol);
                if (diff > 0)
                {
                    lockedLeft.Add(new AssetValue(assetValue.Symbol, diff));
                }
            }

            var freeRight = new List<AssetValue>();
            foreach (var assetValue in other.GetAllFreeBalances())
            {
                var diff = assetValue.Amount - GetFreeBalance(assetValue.Symbol);
                if (diff > 0)
                {
                    freeRight.Add(new AssetValue(assetValue.Symbol, diff));
                }
            }

            var lockedRight = new List<AssetValue>();
            foreach (var assetValue in other.GetAllLockedBalances())
            {
                var diff = assetValue.Amount - GetLockedBalance(assetValue.Symbol);
                if (diff > 0)
                {
                    lockedRight.Add(new AssetValue(assetValue.Symbol, diff));
                }
            }

            // Combine free and locked
            freeLeft.AddRange(freeRight);
            lockedLeft.AddRange(lockedRight);

            var dict = new Dictionary<Currency, Tuple<decimal, decimal>>();
            foreach (var freeAssetValue in freeLeft)
            {
                dict.Add(freeAssetValue.Symbol, new Tuple<decimal, decimal>(freeAssetValue.Amount, 0));
            }

            foreach (var lockedAssetValue in lockedLeft)
            {
                if (dict.ContainsKey(lockedAssetValue.Symbol))
                {
                    var tuple = dict[lockedAssetValue.Symbol];
                    dict[lockedAssetValue.Symbol] = new Tuple<decimal, decimal>(tuple.Item1, lockedAssetValue.Amount);
                }
                else
                {
                    dict.Add(lockedAssetValue.Symbol, new Tuple<decimal, decimal>(lockedAssetValue.Amount, 0));
                }
            }

            List<ExchangeBalance> ebs = new List<ExchangeBalance>();
            foreach (var tuple in dict)
            {
                ebs.Add(new ExchangeBalance(tuple.Key, tuple.Value.Item1, tuple.Value.Item2));
            }

            return new Assets(ebs);
        }
    }
}