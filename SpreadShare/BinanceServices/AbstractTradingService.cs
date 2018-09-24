using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractTradingService : ITradingService
    {
        public abstract ResponseObject Start();
        public abstract long PlaceMarketOrder(string symbol, OrderSide side, decimal amount);
        public abstract void CancelOrder(string symbol, long orderId);
        public abstract decimal GetCurrentPrice(string symbol);
        public abstract decimal GetPerformancePastHours(string symbol, double hoursBack, DateTime endTime);
        public abstract Tuple<string, decimal> GetTopPerformance(double hoursBack, DateTime endTime);
    }
}