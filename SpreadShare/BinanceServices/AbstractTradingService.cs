using Binance.Net.Objects;

namespace SpreadShare.BinanceServices
{
    abstract class AbstractTradingService : ITradingService
    {
        public abstract void Start();
        public abstract long PlaceMarketOrder(string symbol, OrderSide side, decimal amount);
        public abstract void CancelOrder(string symbol, long orderId);
        public abstract decimal GetPrice(string symbol);
    }
}