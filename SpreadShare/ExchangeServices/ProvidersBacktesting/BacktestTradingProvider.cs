using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider, IObserver<long>
    {
        private readonly ILogger _logger;
        private readonly BacktestOutputLogger _backtestOutputLogger;
        private readonly BacktestTimerProvider _timer;
        private readonly BacktestDataProvider _dataProvider;
        private List<OrderUpdate> _orderList;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="timer">timer provider for registering trades</param>
        /// <param name="data">data provider for confirming trades</param>
        public BacktestTradingProvider(ILoggerFactory loggerFactory, BacktestTimerProvider timer, BacktestDataProvider data)
            : base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _timer = timer;
            _dataProvider = data;
            timer.Subscribe(this);

            // Output backtestOutputLogger for writing backtest report
            _backtestOutputLogger = new BacktestOutputLogger();
            _orderList = new List<OrderUpdate>();
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> PlaceFullMarketOrder(TradingPair pair, OrderSide side, decimal amount)
        {
            _backtestOutputLogger.RegisterTradeEvent(_timer.CurrentTime, pair.Right, pair.Left, side, amount, new Currency("BNB"), 69);

            // TODO: retrieve the executed amount
            return new ResponseObject<decimal>(ResponseCode.Success, 0);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public void OnNext(long value)
        {
            foreach (var order in _orderList)
            {
                var query = _dataProvider.GetCurrentPriceLastTrade(order.Pair);
                if (query.Success)
                {
                    bool filled = order.Side == OrderSide.Buy ? query.Data < order.Price : query.Data > order.Price;
                    if (filled)
                    {
                        order.Status = OrderUpdate.OrderStatus.Filled;
                        UpdateObservers(order);
                    }
                }
            }
        }
    }
}