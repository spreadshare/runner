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

            if (side == OrderSide.Sell)
            {
                exec = new TradeExecution(
                    new Balance(pair.Left, amount, 0.0M),
                    new Balance(pair.Right, proposal.From.Free * priceEstimate, 0.0M));
            }

            _comm.RemotePortfolio.UpdateAllocation(exec);

            return new ResponseObject<decimal>(ResponseCode.Success, amount);
        }

        /// <inheritdoc />
        public override ResponseObject PlaceLimitOrder(TradingPair pair, OrderSide side, decimal amount, decimal price)
        {
            // Keep the remote updated by mocking a trade execution
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            TradeExecution exec = null;
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
            _watchList.Add(new OrderUpdate(price, side, OrderUpdate.OrderStatus.New, pair, amount));
            return new ResponseObject(ResponseCode.Success);
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
            foreach (var order in _watchList.ToList())
            {
                var query = _dataProvider.GetCurrentPriceLastTrade(order.Pair);
                if (query.Success)
                {
                    bool filled = order.Side == OrderSide.Buy ? query.Data <= order.SetPrice : query.Data >= order.SetPrice;
                    if (filled && order.Status != OrderUpdate.OrderStatus.Filled)
                    {
                        Logger.LogInformation($"Order confirmed at {_timer.CurrentTime}");
                        order.Status = OrderUpdate.OrderStatus.Filled;

                        // Set the actual price for the order
                        order.AveragePrice = query.Data;
                        order.TotalFilled = order.Amount;
                        order.LastFillIncrement = order.Amount;
                        order.LastFillIncrement = order.Amount;
                        order.LastFillPrice = query.Data;
                        TradeExecution exec = null;
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

                        // TODO: Remove the order from the watch list.
                        UpdateObservers(order);
                    }
                }
            }
        }
    }
}