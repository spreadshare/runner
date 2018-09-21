using System.Collections.Generic;
using Binance.Net.Objects;

namespace SpreadShare.Models
{
    public class Assets
    {
        private readonly Dictionary<string, decimal> _free;
        private readonly Dictionary<string, decimal> _locked;
        private readonly Dictionary<string, decimal> _total;
        public Assets() {
            _free   = new Dictionary<string, decimal>();    
            _locked = new Dictionary<string, decimal>();
            _total  = new Dictionary<string, decimal>();    
        }

        public Assets(List<BinanceBalance> input) : this() {
            foreach(var balance in input) {
                _free.Add(balance.Asset, balance.Free);
                _locked.Add(balance.Asset, balance.Locked);
                _total.Add(balance.Asset, balance.Total);
            }
        }

        public decimal GetFreeBalance(string symbol) 
        {
            return _free.GetValueOrDefault(symbol, 0);
        }

        public decimal GetLockedBalance(string symbol)
        {
            return _locked.GetValueOrDefault(symbol, 0);
        }

        public decimal GetTotalBalance(string symbol)
        {
            return _free.GetValueOrDefault(symbol, 0);
        }

        public List<AssetValue> GetAllFreeBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(var symbol in _free.Keys) {
                if (_free[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, _free[symbol]));
                }
            }
            return ret;
        }

        public List<AssetValue> GetAllLockedBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(var symbol in _locked.Keys) {
                if (_locked[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, _locked[symbol]));
                }
            }
            return ret;
        }

        public List<AssetValue> GetAllTotalBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(var symbol in _total.Keys) {
                if (_total[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, _total[symbol]));
                }
            }
            return ret;
        }
    }

    public struct AssetValue {
        public string Symbol { get; }
        public decimal Value { get; }

        public AssetValue(string symbol, decimal value) {
            Symbol = symbol;
            Value = value;
        }
    }
}