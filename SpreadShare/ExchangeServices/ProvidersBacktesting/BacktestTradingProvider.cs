using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    internal class BacktestTradingProvider : AbstractTradingProvider, IObserver<long>
    {
        private readonly ILogger _logger;
        private readonly BacktestTimerProvider _timer;
        private readonly BacktestDataProvider _dataProvider;
        private readonly BacktestCommunicationService _comm;
        private readonly DatabaseContext _database;

        private long _mockOrderCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="timer">timer provider for registering trades</param>
        /// <param name="data">data provider for confirming trades</param>
        /// <param name="comm">communication service for updating remote portfolio</param>
        /// <param name="database">Database context for logging trades</param>
        public BacktestTradingProvider(
            ILoggerFactory loggerFactory,
            BacktestTimerProvider timer,
            BacktestDataProvider data,
            BacktestCommunicationService comm,
            DatabaseContext database)
            : base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _timer = timer;
            _dataProvider = data;
            _comm = comm;
            _database = database;
            timer.Subscribe(this);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceMarketOrder(TradingPair pair, OrderSide side, decimal quantity)
        {
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0.0M));

            // Keep the remote updated by mocking a trade execution and letting the communications know.
            TradeExecution exec = null;
            decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
            if (side == OrderSide.Buy)
            {
                exec = new TradeExecution(
                    new Balance(pair.Right, proposal.From.Free * priceEstimate, 0.0M),
                    new Balance(pair.Left, quantity, 0.0M));
            }
            else
            {
                exec = new TradeExecution(
                    new Balance(pair.Left, quantity, 0.0M),
                    new Balance(pair.Right, proposal.From.Free * priceEstimate, 0.0M));
            }

            _comm.RemotePortfolio.UpdateAllocation(exec);

            var orderUpdate = new OrderUpdate(
                _mockOrderCounter++,
                OrderUpdate.OrderTypes.Market,
                _timer.CurrentTime.ToUnixTimeMilliseconds(),
                priceEstimate,
                side,
                pair,
                quantity)
            {
                Status = OrderUpdate.OrderStatus.Filled,
                AverageFilledPrice = priceEstimate,
                FilledQuantity = quantity,
                FilledTimeStamp = _timer.CurrentTime.ToUnixTimeMilliseconds()
            };

            // Write the trade to the database
            _database.Trades.Add(new DatabaseTrade(
                orderUpdate,
                _comm.RemotePortfolio.ToJson(),
                _dataProvider.ValuatePortfolioInBaseCurrency(_comm.RemotePortfolio)));

            return new ResponseObject<OrderUpdate>(
                ResponseCode.Success,
                orderUpdate);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price)
        {
            // Keep the remote updated by mocking a trade execution
            TradeExecution exec;

            // Mock the remote portfolio by providing it an update
            if (side == OrderSide.Buy)
            {
                exec = new TradeExecution(
                    new Balance(pair.Right, quantity * price, 0),
                    new Balance(pair.Right, 0, quantity * price));
            }
            else
            {
                exec = new TradeExecution(
                    new Balance(pair.Left, quantity, 0),
                    new Balance(pair.Left, 0, quantity));
            }

            _comm.RemotePortfolio.UpdateAllocation(exec);

            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(
                _mockOrderCounter,
                OrderUpdate.OrderTypes.Limit,
                _timer.CurrentTime.ToUnixTimeMilliseconds(),
                price,
                side,
                pair,
                quantity);
            WatchList.Add(_mockOrderCounter, order);
            _mockOrderCounter++;

            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            var order = GetOrderInfo(pair, orderId).Data;
            order.Status = OrderUpdate.OrderStatus.Cancelled;
            order.FilledTimeStamp = _timer.CurrentTime.ToUnixTimeMilliseconds();

            if (WatchList.ContainsKey(orderId))
            {
                WatchList.Remove(orderId);
            }

            // Update the remote portfolio
            TradeExecution exec;
            if (order.Side == OrderSide.Buy)
            {
                exec = new TradeExecution(
                    new Balance(order.Pair.Right, 0, order.SetQuantity * order.SetPrice),
                    new Balance(order.Pair.Right, order.SetQuantity * order.SetPrice, 0));
            }
            else
            {
                exec = new TradeExecution(
                    new Balance(order.Pair.Left, 0, order.SetQuantity),
                    new Balance(order.Pair.Left, order.SetQuantity, 0));
            }

            _logger.LogInformation($"Updating remote with exec {JsonConvert.SerializeObject(exec)}");
            _comm.RemotePortfolio.UpdateAllocation(exec);

            // Add cancelled order to the database
            _database.Trades.Add(new DatabaseTrade(
                order,
                _comm.RemotePortfolio.ToJson(),
                _dataProvider.ValuatePortfolioInBaseCurrency(_comm.RemotePortfolio)));

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
        public void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public void OnNext(long value)
        {
            foreach (var order in WatchList.Values.ToList())
            {
                decimal price = _dataProvider.GetCurrentPriceLastTrade(order.Pair).Data;
                if (!FilledLimitOrder(order))
                {
                    continue;
                }

                Logger.LogInformation($"Order {order.OrderId} confirmed at {_timer.CurrentTime}");
                order.Status = OrderUpdate.OrderStatus.Filled;

                // Set the actual price for the order
                order.AverageFilledPrice = price;
                order.FilledQuantity = order.SetQuantity;
                order.LastFillIncrement = order.SetQuantity;
                order.LastFillPrice = price;
                order.FilledTimeStamp = _timer.CurrentTime.ToUnixTimeMilliseconds();

                // Calculate a trade execution to keep the remote portfolio up-to-date
                TradeExecution exec;
                if (order.Side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Right, 0, order.SetQuantity * order.SetPrice),
                        new Balance(order.Pair.Left, order.LastFillIncrement, 0));
                }
                else
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Left, 0, order.LastFillIncrement),
                        new Balance(order.Pair.Right, order.SetQuantity * order.AverageFilledPrice, 0));
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

        private bool FilledLimitOrder(OrderUpdate order)
        {
            decimal price = _dataProvider.GetCurrentPriceLastTrade(order.Pair).Data;
            return order.Side == OrderSide.Buy
                ? price <= order.SetPrice
                : price >= order.SetPrice;
        }
    }
}