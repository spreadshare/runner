using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractTradingService : ITradingService
    {
        public abstract ResponseObject Start();
        public abstract ResponseObject<long> PlaceMarketOrder(string symbol, OrderSide side, decimal amount);
        public abstract ResponseObject CancelOrder(string symbol, long orderId);
        public abstract ResponseObject<decimal> GetCurrentPrice(string symbol);
        public abstract ResponseObject<decimal> GetPerformancePastHours(string symbol, double hoursBack, DateTime endTime);
        public abstract ResponseObject<Tuple<string, decimal>> GetTopPerformance(double hoursBack, DateTime endTime);
    }
}