using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using OrderSide = SpreadShare.Models.OrderSide;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider
    {
        private readonly ILogger _logger;
        private readonly BacktestDataProvider _dataProvider;
        private readonly BacktestCommunicationService _comm;
        private readonly DatabaseContext _database;
        private readonly Queue<OrderUpdate> _orderCache;

        private long _mockOrderCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="timer">Timer provider for registering trades.</param>
        /// <param name="data">Data provider for confirming trades.</param>
        /// <param name="comm">Communication service for updating remote portfolio.</param>
        /// <param name="database">Database context for logging trades.</param>
        public BacktestTradingProvider(
            ILoggerFactory loggerFactory,
            BacktestTimerProvider timer,
            BacktestDataProvider data,
            BacktestCommunicationService comm,
            DatabaseContext database)
            : base(loggerFactory, timer)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _dataProvider = data;
            _comm = comm;
            _database = database;
            _orderCache = new Queue<OrderUpdate>();
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> ExecuteMarketOrder(TradingPair pair, OrderSide side, decimal quantity, long tradeId)
        {
            decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;

            var order = new OrderUpdate(
                _mockOrderCounter++,
                tradeId,
                OrderUpdate.OrderStatus.Filled,
                OrderUpdate.OrderTypes.Market,
                Timer.CurrentTime.ToUnixTimeMilliseconds(),
                priceEstimate,
                side,
                pair,
                quantity)
            {
                AverageFilledPrice = priceEstimate,
                FilledQuantity = quantity,
                FilledTimeStamp = Timer.CurrentTime.ToUnixTimeMilliseconds(),
            };

            // Write the trade to the database
            _database.Trades.Add(new DatabaseTrade(
                order,
                _comm.RemotePortfolio.ToJson(),
                _dataProvider.ValuatePortfolioInBaseCurrency(_comm.RemotePortfolio)));

            _comm.RemotePortfolio.UpdateAllocation(TradeExecution.FromOrder(order));

            return new ResponseObject<OrderUpdate>(
                ResponseCode.Success,
                order);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(
                _mockOrderCounter,
                tradeId,
                OrderUpdate.OrderStatus.New,
                OrderUpdate.OrderTypes.Limit,
                Timer.CurrentTime.ToUnixTimeMilliseconds(),
                price,
                side,
                pair,
                quantity);
            WatchList.Add(_mockOrderCounter, order);
            _mockOrderCounter++;

            _comm.RemotePortfolio.UpdateAllocation(TradeExecution.FromOrder(order));
            _orderCache.Enqueue(order);

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(
                _mockOrderCounter,
                tradeId,
                OrderUpdate.OrderStatus.New,
                OrderUpdate.OrderTypes.StopLoss,
                Timer.CurrentTime.ToUnixTimeMilliseconds(),
                price,
                side,
                pair,
                quantity);
            WatchList.Add(_mockOrderCounter, order);
            _mockOrderCounter++;

            _comm.RemotePortfolio.UpdateAllocation(TradeExecution.FromOrder(order));
            _orderCache.Enqueue(order);

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            var order = GetOrderInfo(pair, orderId).Data;
            order.Status = OrderUpdate.OrderStatus.Cancelled;
            order.FilledTimeStamp = Timer.CurrentTime.ToUnixTimeMilliseconds();

            if (WatchList.ContainsKey(orderId))
            {
                WatchList.Remove(orderId);
            }

            // Add cancelled order to the database
            _database.Trades.Add(new DatabaseTrade(
                order,
                _comm.RemotePortfolio.ToJson(),
                _dataProvider.ValuatePortfolioInBaseCurrency(_comm.RemotePortfolio)));

            _comm.RemotePortfolio.UpdateAllocation(TradeExecution.FromOrder(order));
            UpdateObservers(order);

            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> GetOrderInfo(TradingPair pair, long orderId)
        {
            if (WatchList.ContainsKey(orderId))
            {
                return new ResponseObject<OrderUpdate>(ResponseCode.Success, WatchList[orderId]);
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.Error, $"Order {orderId} was not found");
        }

        /// <inheritdoc />
        public override void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public override void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public override void OnNext(long value)
        {
            while (_orderCache.TryDequeue(out var cachedOrder))
            {
                UpdateObservers(cachedOrder);
            }

            foreach (var order in WatchList.Values.ToList())
            {
                decimal price = _dataProvider.GetCurrentPriceLastTrade(order.Pair).Data;
                if (order.OrderType == OrderUpdate.OrderTypes.Limit && !FilledLimitOrder(order, price))
                {
                    continue;
                }

                if (order.OrderType == OrderUpdate.OrderTypes.StopLoss && !FilledStoplossOrder(order, price))
                {
                    continue;
                }

                Logger.LogInformation($"Order {order.OrderId} confirmed at {Timer.CurrentTime}");
                order.Status = OrderUpdate.OrderStatus.Filled;

                // Set the actual price for the order
                order.AverageFilledPrice = order.SetPrice;
                order.FilledQuantity = order.SetQuantity;
                order.LastFillIncrement = order.SetQuantity;
                order.LastFillPrice = order.SetPrice;
                order.FilledTimeStamp = Timer.CurrentTime.ToUnixTimeMilliseconds();

                // Calculate a trade execution to keep the remote portfolio up-to-date
                TradeExecution exec;
                if (order.Side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Right, 0.0M, order.SetQuantity * order.SetPrice),
                        new Balance(order.Pair.Left, order.LastFillIncrement, 0.0M));
                }
                else
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Left, 0.0M, order.LastFillIncrement),
                        new Balance(order.Pair.Right, order.SetQuantity * order.AverageFilledPrice, 0.0M));
                }

                _comm.RemotePortfolio.UpdateAllocation(exec);

                // Write the filled trade to the database
                _database.Trades.Add(new DatabaseTrade(
                    order,
                    _comm.RemotePortfolio.ToJson(),
                    _dataProvider.ValuatePortfolioInBaseCurrency(_comm.RemotePortfolio)));

                UpdateObservers(order);
            }

            // Clean up filled orders
            WatchList = WatchList.Where(keyPair => keyPair.Value.Status != OrderUpdate.OrderStatus.Filled)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        private static bool FilledLimitOrder(OrderUpdate order, decimal price)
        {
            return order.Side == OrderSide.Buy
                ? price <= order.SetPrice
                : price >= order.SetPrice;
        }

        private static bool FilledStoplossOrder(OrderUpdate order, decimal price)
        {
            return order.Side == OrderSide.Buy
                ? price >= order.SetPrice
                : price <= order.SetPrice;
        }
    }
}