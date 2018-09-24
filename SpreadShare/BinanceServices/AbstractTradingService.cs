using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractTradingService : ITradingService
    {
        public abstract ResponseObject Start();
        protected abstract long PlaceMarketOrder(string symbol, OrderSide side, decimal amount);
        public abstract ResponseObject ChangeEntirePosition(string symbol);
        public abstract decimal GetPrice(string symbol);
    }
}