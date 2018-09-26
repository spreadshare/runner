using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractTradingService : ITradingService
    {
        protected enum TradeState { Received, Executed, Canceled, Expired, Rejected, Unknown };
        public abstract ResponseObject Start();
        public abstract ResponseObject PlaceFullMarketOrder(string symbol, OrderSide side);
        public abstract ResponseObject CancelOrder(string symbol, long orderId);
        public abstract ResponseObject<decimal> GetCurrentPrice(string symbol);
        public abstract ResponseObject<decimal> GetPerformancePastHours(string symbol, double hoursBack, DateTime endTime);
        public abstract ResponseObject<Tuple<string, decimal>> GetTopPerformance(double hoursBack, DateTime endTime);
    }
}