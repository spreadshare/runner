using System.Collections.Generic;
using Binance.Net.Objects;

namespace SpreadShare.Models
{
    public class Assets
    {
        private Dictionary<string, decimal> free;
        private Dictionary<string, decimal> locked;
        private Dictionary<string, decimal> total;
        public Assets() {
            free   = new Dictionary<string, decimal>();    
            locked = new Dictionary<string, decimal>();
            total  = new Dictionary<string, decimal>();    
        }

        public Assets(List<BinanceBalance> input) : this() {
            foreach(BinanceBalance balance in input) {
                free.Add(balance.Asset, balance.Free);
                locked.Add(balance.Asset, balance.Locked);
                total.Add(balance.Asset, balance.Total);
            }
        }

        public decimal GetFreeBalance(string symbol) 
        {
            return free.GetValueOrDefault(symbol, 0);
        }

        public decimal GetLockedBalance(string symbol)
        {
            return locked.GetValueOrDefault(symbol, 0);
        }

        public decimal GetTotalBalance(string symbol)
        {
            return free.GetValueOrDefault(symbol, 0);
        }

        public List<AssetValue> GetAllFreeBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(string symbol in free.Keys) {
                if (free[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, free[symbol]));
                }
            }
            return ret;
        }

        public List<AssetValue> GetAllLockedBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(string symbol in locked.Keys) {
                if (locked[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, locked[symbol]));
                }
            }
            return ret;
        }

        public List<AssetValue> GetAllTotalBalances() {
            List<AssetValue> ret = new List<AssetValue>();
            foreach(string symbol in total.Keys) {
                if (total[symbol] > 0) {
                    ret.Add(new AssetValue(symbol, total[symbol]));
                }
            }
            return ret;
        }
    }

    public struct AssetValue {
        string symbol;
        decimal value;
        public string Symbol { get { return symbol; }}
        public decimal Value { get { return value; }}

        public AssetValue(string symbol, decimal value) {
            this.symbol = symbol;
            this.value = value;
        }
    }
}