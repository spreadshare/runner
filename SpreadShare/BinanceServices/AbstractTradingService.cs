using Binance.Net.Objects;

namespace SpreadShare.BinanceServices
{
    abstract class AbstractTradingService : ITradingService
    {
        public abstract void Start();
        public abstract long PlaceMarketOrder(string symbol, OrderSide side);
        public abstract void CancelOrder(string symbol, long orderId);
    }
}