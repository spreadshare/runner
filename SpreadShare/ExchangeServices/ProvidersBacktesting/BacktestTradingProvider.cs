using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Clauses;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
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
        private readonly BacktestTimerProvider _timer;
        private readonly BacktestDataProvider _dataProvider;
        private readonly BacktestCommunicationService _comm;
        private long _mockOrderCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="timer">timer provider for registering trades</param>
        /// <param name="data">data provider for confirming trades</param>
        /// <param name="comm">communication service for updating remote portfolio</param>
        public BacktestTradingProvider(
            ILoggerFactory loggerFactory,
            BacktestTimerProvider timer,
            BacktestDataProvider data,
            BacktestCommunicationService comm)
            : base(loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _timer = timer;
            _dataProvider = data;
            _comm = comm;
            timer.Subscribe(this);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> PlaceFullMarketOrder(TradingPair pair, OrderSide side, decimal amount)
        {
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            var proposal = new TradeProposal(new Balance(currency, amount, 0.0M));

            // Keep the remote updated by mocking a trade execution and letting the communications know.
            TradeExecution exec = null;
            decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
            if (side == OrderSide.Buy)
            {
                exec = new TradeExecution(
                    new Balance(pair.Right, proposal.From.Free * priceEstimate, 0.0M),
                    new Balance(pair.Left, amount, 0.0M));
            }
            else
            {
                exec = new TradeExecution(
                    new Balance(pair.Left, amount, 0.0M),
                    new Balance(pair.Right, proposal.From.Free * priceEstimate, 0.0M));
            }

            _comm.RemotePortfolio.UpdateAllocation(exec);

            return new ResponseObject<decimal>(ResponseCode.Success, amount);
        }

        /// <inheritdoc />
        public override ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal amount, decimal price)
        {
            // Keep the remote updated by mocking a trade execution
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            TradeExecution exec = null;
            
            // Mock the remote portfolio by providing it an update
            if (side == OrderSide.Buy)
            {
                exec = new TradeExecution(
                    new Balance(pair.Right, amount * price, 0),
                    new Balance(pair.Right, 0, amount * price));
            }

            if (side == OrderSide.Sell)
            {
                exec = new TradeExecution(
                    new Balance(pair.Left, amount, 0),
                    new Balance(pair.Left, 0, amount));
            }

            _comm.RemotePortfolio.UpdateAllocation(exec);

            // Add the order to the watchlist
            OrderUpdate order = new OrderUpdate(price, side, pair, amount, _mockOrderCounter);
            _watchList.Add(_mockOrderCounter, order);
            _mockOrderCounter++;
            return new ResponseObject<OrderUpdate>(ResponseCode.Success, order);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public void OnNext(long value)
        {
            foreach (var order in _watchList.Values.ToList())
            {
                decimal price = _dataProvider.GetCurrentPriceLastTrade(order.Pair).Data;
                if (!FilledLimitOrder(order))
                {
                    continue;
                }

                Logger.LogInformation($"Order {order.OrderId} confirmed at {_timer.CurrentTime}");
                order.Status = OrderUpdate.OrderStatus.Filled;

                // Set the actual price for the order
                order.AveragePrice = price;
                order.TotalFilled = order.Amount;
                order.LastFillIncrement = order.Amount;
                order.LastFillPrice = price;

                // Calculate a trade execution to keep the remote portfolio up-to-date
                TradeExecution exec;
                if (order.Side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Right, 0, order.Amount * order.SetPrice),
                        new Balance(order.Pair.Left, order.LastFillIncrement, 0));
                }
                else
                {
                    exec = new TradeExecution(
                        new Balance(order.Pair.Left, 0, order.LastFillIncrement),
                        new Balance(order.Pair.Right, order.Amount * order.AveragePrice, 0));
                }

                _comm.RemotePortfolio.UpdateAllocation(exec);

                UpdateObservers(order);
            }

            // Clean up filled orders
            _watchList = _watchList.Where(keyPair => keyPair.Value.Status != OrderUpdate.OrderStatus.Filled)
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