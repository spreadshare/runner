using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
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
            decimal executed = side == OrderSide.Buy
                ? amount
                : amount * _dataProvider.GetCurrentPriceLastTrade(pair).Data;

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

        public override ResponseObject PlaceLimitOrder(TradingPair pair, OrderSide side, decimal amount, decimal price)
        {
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
            foreach (var order in _watchList)
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