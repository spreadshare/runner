using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using OrderSide = SpreadShare.Models.Trading.OrderSide;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider
    {
        private readonly ILogger _logger;
        private readonly BacktestDataProvider _dataProvider;
        private readonly DatabaseContext _database;

        /// <summary>
        /// This queue is used to cache orders until the next clock tick. It is used to confirm order placements.
        /// </summary>
        private readonly Queue<OrderUpdate> _orderCache;

        private long _mockOrderCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="timer">Timer provider for registering trades.</param>
        /// <param name="data">Data provider for confirming trades.</param>
        /// <param name="database">Database context for logging trades.</param>
        public BacktestTradingProvider(
            ILoggerFactory loggerFactory,
            BacktestTimerProvider timer,
            BacktestDataProvider data,
            DatabaseContext database)
            : base(loggerFactory, timer)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _dataProvider = data;
            _database = database;
            _orderCache = new Queue<OrderUpdate>();
        }

        /// <summary>
        /// Gets or sets the parent implementation, needed to get the portfolio.
        /// </summary>
        public TradingProvider ParentImplementation { get; set; }

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

            // Add to order cache to confirm filled
            _orderCache.Enqueue(order);

            // Write the trade to the logger
            LogOrder(order, ParentImplementation.GetPortfolio().Clone());

            return new ResponseObject<OrderUpdate>(
                ResponseCode.Success,
                order);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(
                _mockOrderCounter++,
                tradeId,
                OrderUpdate.OrderStatus.New,
                OrderUpdate.OrderTypes.Limit,
                Timer.CurrentTime.ToUnixTimeMilliseconds(),
                price,
                side,
                pair,
                quantity);

            // Add to order cache to confirm placement
            _orderCache.Enqueue(order);

            // Add to watch list to check if filled
            WatchList.Add(order.OrderId, order);

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceStoplossOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price, long tradeId)
        {
            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(
                orderId: _mockOrderCounter++,
                tradeId: tradeId,
                orderStatus: OrderUpdate.OrderStatus.New,
                orderType: OrderUpdate.OrderTypes.StopLoss,
                createdTimeStamp: Timer.CurrentTime.ToUnixTimeMilliseconds(),
                setPrice: 0,
                side: side,
                pair: pair,
                setQuantity: quantity)
            {
                StopPrice = price,
            };

            // Add to order cache to confirm placement.
            _orderCache.Enqueue(order);

            // Add to watchlist to check if filled
            WatchList.Add(order.OrderId, order);

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            if (!WatchList.ContainsKey(orderId))
            {
                throw new InvalidStateException($"Cannot cancel order {orderId} because it doesn't exist.");
            }

            var order = WatchList[orderId];
            order.Status = OrderUpdate.OrderStatus.Cancelled;
            order.FilledTimeStamp = Timer.CurrentTime.ToUnixTimeMilliseconds();
            WatchList.Remove(order.OrderId);

            // Add to order cache to confirm cancelled.
            _orderCache.Enqueue(order);

            // Add cancelled order to the logger
            LogOrder(order, ParentImplementation.GetPortfolio().Clone());

            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> WaitForOrderStatus(long orderId, OrderUpdate.OrderStatus status)
        {
            while (_orderCache.TryDequeue(out var order))
            {
                UpdateObservers(order);
                if (order.OrderId == orderId && order.Status == status)
                {
                    return new ResponseObject<OrderUpdate>(order);
                }
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.Error, "Backtest order was not added to the cache correctly");
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

                if (GetFilledOrder(order, price, Timer.CurrentTime.ToUnixTimeMilliseconds()))
                {
                    Logger.LogInformation($"Order {order.OrderId} confirmed at {Timer.CurrentTime}");

                    // Write the filled trade to the logger
                    LogOrder(order, ParentImplementation.GetPortfolio().Clone());

                    UpdateObservers(order);
                }
            }

            // Clean up filled orders
            WatchList = WatchList
                .Where(keyPair => !keyPair.Value.Finalized)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Returns a bool indicating whether or not the order was filled, and transforms the order into a filled state if so.
        /// </summary>
        /// <param name="order">The order to check as filled.</param>
        /// <param name="currentPrice">The price to check the order against.</param>
        /// <param name="currentTime">The current time (will be used as time of fill).</param>
        /// <returns>Whether the order was filled.</returns>
        /// <exception cref="UnexpectedOrderTypeException">The order type is not supported by the method.</exception>
        private static bool GetFilledOrder(OrderUpdate order, decimal currentPrice, long currentTime)
        {
            if (order == null)
            {
                return false;
            }

            switch (order.OrderType)
            {
                case OrderUpdate.OrderTypes.Limit:
                    return GetFilledLimitOrder(order, currentPrice, currentTime);
                case OrderUpdate.OrderTypes.StopLoss:
                    return GetFilledStoplossOrder(order, currentPrice, currentTime);
                default:
                    throw new UnexpectedOrderTypeException(
                        $"Backtest watchlist should not contain order of type {order.OrderType}");
            }
        }

        private static bool GetFilledStoplossOrder(OrderUpdate order, decimal currentPrice, long currentTime)
        {
            bool filled = order.Side == OrderSide.Buy
                ? currentPrice >= order.StopPrice
                : currentPrice <= order.StopPrice;
            if (filled)
            {
                order.StopPrice = order.StopPrice;
                order.FilledQuantity = order.SetQuantity;
                order.AverageFilledPrice = order.StopPrice;
                order.LastFillIncrement = order.SetQuantity;
                order.LastFillPrice = order.StopPrice;
                order.Status = OrderUpdate.OrderStatus.Filled;
                order.FilledTimeStamp = currentTime;
                return true;
            }

            return false;
        }

        private static bool GetFilledLimitOrder(OrderUpdate order, decimal currentPrice, long currentTime)
        {
            bool filled = order.Side == OrderSide.Buy
                ? currentPrice <= order.SetPrice
                : currentPrice >= order.SetPrice;
            if (filled)
            {
                order.StopPrice = order.StopPrice;
                order.FilledQuantity = order.SetQuantity;
                order.AverageFilledPrice = order.SetPrice;
                order.LastFillIncrement = order.SetQuantity;
                order.LastFillPrice = order.SetPrice;
                order.Status = OrderUpdate.OrderStatus.Filled;
                order.FilledTimeStamp = currentTime;
                return true;
            }

            return false;
        }

        private void LogOrder(OrderUpdate order, Portfolio portfolio)
        {
            ((BacktestTimerProvider)Timer).AddOrder(
                new BacktestOrder(
                    order,
                    JsonConvert.SerializeObject(portfolio),
                    _dataProvider.ValuatePortfolioInBaseCurrency(portfolio)));
        }
    }
}