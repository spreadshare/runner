using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects;

namespace SpreadShare.Models
{
    /// <summary>
    /// Represents the portfolio of an exchange wallet.
    /// </summary>
    internal class Assets
    {
        private readonly Dictionary<string, decimal> _free;
        private readonly Dictionary<string, decimal> _locked;
        private readonly Dictionary<string, decimal> _total;

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets"/> class.
        /// </summary>
        public Assets()
        {
            _free = new Dictionary<string, decimal>();
            _locked = new Dictionary<string, decimal>();
            _total = new Dictionary<string, decimal>();
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
            return _free.GetValueOrDefault(symbol.ToString(), 0);
        }

        /// <summary>
        /// Gets the balance of a currency that is locked
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The locked balance of a currency</returns>
        public decimal GetLockedBalance(Currency symbol)
        {
            return _locked.GetValueOrDefault(symbol.ToString(), 0);
        }

        /// <summary>
        /// Gets the total balance of a currency
        /// </summary>
        /// <param name="symbol">Symbol of the currency</param>
        /// <returns>The total balance of a currency</returns>
        public decimal GetTotalBalance(Currency symbol)
        {
            return _free.GetValueOrDefault(symbol.ToString(), 0);
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
    }
}