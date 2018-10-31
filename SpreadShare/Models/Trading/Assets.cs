using System;
using System.Collections.Generic;
using System.Linq;

namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Represents the portfolio of an exchange wallet.
    /// </summary>
    internal class Assets
    {
        private readonly Dictionary<Currency, Balance> _balances;

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets"/> class.
        /// </summary>
        public Assets()
        {
            _balances = new Dictionary<Currency, Balance>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets"/> class.
        /// </summary>
        /// <param name="input">List of balances</param>
        public Assets(List<Balance> input)
            : this()
        {
            foreach (var balance in input)
            {
                _balances.Add(balance.Symbol, balance);
            }
        }

        /// <summary>
        /// Gets the balance of a currency that is unallocated
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The unallocated balance of a currency</returns>
        public decimal GetFreeBalance(Currency symbol)
        {
            if (!_balances.ContainsKey(symbol))
            {
                return 0.0M;
            }

            return _balances[symbol].Free;
        }

        /// <summary>
        /// Gets the balance of a currency that is locked
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The locked balance of a currency</returns>
        public decimal GetLockedBalance(Currency symbol)
        {
            if (!_balances.ContainsKey(symbol))
            {
                return 0.0M;
            }

            return _balances[symbol].Locked;
        }

        public List<Currency> GetCurrencies()
        {
            return _balances.Keys.ToList();
        }

        public Assets Add(Assets other)
        {
            if (other == null)
            {
                return this;
            }

            var result = new Assets(_balances.Values.ToList());

            foreach (var currency in other.GetCurrencies())
            {
                var allKeys = GetCurrencies();
                allKeys.AddRange(other.GetCurrencies());
            }



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
                .Select(assetValue => new Balance(
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

            List<Balance> result = new List<Balance>();
            var balancesThis = this.GetExchangeBalances();

            // Result += contains all Other.Currencies
            foreach (var balance in other.GetExchangeBalances())
            {
                // Get [Symbol, Free, Locked] from current balances
                Balance temp = balancesThis.SingleOrDefault(b => b.Symbol.Equals(balance.Symbol));
                if (temp == null)
                {
                    temp = new Balance(balance.Symbol, 0.0M, 0.0M);
                }

                result.Add(new Balance(
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

                result.Add(new Balance(balance.Symbol, balance.Free, balance.Locked));
            }

            return new Assets(result);
        }

        private List<Balance> GetExchangeBalances()
        {
            List<Balance> balances = new List<Balance>();

            foreach (var balance in GetAllTotalBalances())
            {
                balances.Add(
                    new Balance(
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
            return new Assets(ebs);
        }
    }
}