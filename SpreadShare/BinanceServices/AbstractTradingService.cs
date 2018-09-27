using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractTradingService : ITradingService
    {
        protected enum TradeState { Received, Executed, Canceled, Expired, Rejected, Unreceived };
        public abstract ResponseObject Start();
        public abstract ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side);
        public abstract ResponseObject CancelOrder(CurrencyPair pair, long orderId);
        public abstract ResponseObject<decimal> GetCurrentPrice(CurrencyPair pair);
        public abstract ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTime endTime);
        public abstract ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(double hoursBack, DateTime endTime);
    }
}