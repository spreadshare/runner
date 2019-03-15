using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
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
            var container = ExchangeFactoryService.BuildContainer<TemplateAlgorithm>(AlgorithmConfiguration);
            var trading = container.TradingProvider;
            var property = trading.GetType().GetProperty("Implementation", BindingFlags.NonPublic | BindingFlags.Instance)
                           ?? throw new Exception($"Expected property 'Implementation' on {nameof(TradingProvider)}");

            // Inject test implementation
            var implementation = Activator.CreateInstance(typeof(T), LoggerFactory, container.TimerProvider);
            property.SetValue(trading, implementation);
            return trading;
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
    }
}