using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.Tests.Stubs;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices.TradingProviderTests
{
    public abstract class TradingProviderTestUtils : BaseProviderTests
    {
        protected TradingProviderTestUtils(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        internal TradingProvider GetTradingProvider<T>()
            where T : TradingProviderTestImplementation
        {
            var timer = new TestTimerProvider(LoggerFactory);
            var implementation = (T)Activator.CreateInstance(typeof(T), LoggerFactory, timer);
            var data = new DataProvider(LoggerFactory, new DataProviderTestImplementation(LoggerFactory, timer), AlgorithmConfiguration);
            return new TradingProvider(LoggerFactory, implementation, data, new TestAllocationManager());
        }

        internal abstract class TradingProviderTestImplementation : AbstractTradingProvider
        {
            protected TradingProviderTestImplementation(ILoggerFactory loggerFactory, TimerProvider timer)
                : base(loggerFactory, timer)
            {
                Cache = new List<OrderUpdate>();
            }

            protected abstract List<OrderUpdate> Cache { get; set; }

            public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId) => throw new NotImplementedException();

            public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId) => throw new NotImplementedException();

            public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId) => throw new NotImplementedException();

            public override ResponseObject CancelOrder(TradingPair pair, long orderId) => throw new NotImplementedException();

            public sealed override ResponseObject<OrderUpdate> WaitForOrderStatus(long orderId, OrderUpdate.OrderStatus status)
            {
                foreach (var order in Cache)
                {
                    if (order.OrderId == orderId && order.Status == status)
                    {
                        return new ResponseObject<OrderUpdate>(order);
                    }
                }

                return new ResponseObject<OrderUpdate>(ResponseCode.Error);
            }

            public override void OnCompleted() => throw new NotImplementedException();

            public override void OnError(Exception error) => throw new NotImplementedException();

            public override void OnNext(long value) => throw new NotImplementedException();
        }

        private class DataProviderTestImplementation : AbstractDataProvider
        {
            public DataProviderTestImplementation(ILoggerFactory loggerFactory, TimerProvider timerProvider)
                : base(loggerFactory, timerProvider)
            {
            }

            public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair) => new ResponseObject<decimal>(1M);

            public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => new ResponseObject<decimal>(1M);

            public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => new ResponseObject<decimal>(1M);

            public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack) => throw new NotImplementedException();

            public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack) => throw new NotImplementedException();

            protected override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit) => throw new NotImplementedException();
        }
    }
}