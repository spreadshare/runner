using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Provides trading capabilities.
    /// </summary>
    internal class TradingProvider : Observable<OrderUpdate>
    {
        private readonly ILogger _logger;
        private readonly AbstractTradingProvider _implementation;
        private readonly WeakAllocationManager _allocationManager;
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="implementation">The implementationt to delegate calls to</param>
        /// <param name="dataProvider">The data provider to manager certain orders with</param>
        /// <param name="allocationManager">The allocation manager to verify orders</param>
        /// <param name="algorithm">The type of the algorithm</param>
        /// <param name="exchange">The exchange to provide in question</param>
        public TradingProvider(
            ILoggerFactory loggerFactory,
            AbstractTradingProvider implementation,
            DataProvider dataProvider,
            WeakAllocationManager allocationManager)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _implementation = implementation;
            _allocationManager = allocationManager;
            _dataProvider = dataProvider;
            _implementation.Subscribe(new ConfigurableObserver<OrderUpdate>(
                UpdateAllocation,
                () => { },
                e => { }));
        }

        /// <summary>
        /// Gets the portfolio associated with an algorithm
        /// </summary>
        /// <returns>Response object indicating success or not</returns>
        /// TODO: Make method algorithm specific
        public Portfolio GetPortfolio()
        {
            return _allocationManager.GetAllFunds();
        }

        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">trading pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public ResponseObject<OrderUpdate> PlaceFullMarketOrder(TradingPair pair, OrderSide side)
        {
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            Balance amount = _allocationManager.GetAvailableFunds(currency);
            var proposal = new TradeProposal(new Balance(currency, amount.Free, 0.0M));

            ResponseObject<OrderUpdate> query = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            var tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                query = RetryMethod(() =>
                {
                    decimal tradeAmount = side == OrderSide.Buy
                        ? GetBuyAmountEstimate(pair, proposal.From.Free)
                        : proposal.From.Free;
                    return _implementation.PlaceFullMarketOrder(pair, side, tradeAmount);
                });

                if (!query.Success)
                {
                    return null;
                }

                // Report the trade with the actual amount as communicated by the exchange.
                TradeExecution exec;
                if (side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(pair.Right, proposal.From.Free, 0.0M),
                        new Balance(pair.Left, query.Data.Amount, 0.0M));
                }
                else
                {
                    decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
                    exec = new TradeExecution(
                        new Balance(pair.Left, proposal.From.Free, 0.0M),
                        new Balance(pair.Right, query.Data.Amount * priceEstimate, 0.0M));
                }

                return exec;
            });

            return query;
        }

        /// <summary>
        /// Place a limit order at a certain price
        /// </summary>
        /// <param name="pair">Trading Pair</param>
        /// <param name="side">Buy or sell</param>
        /// <param name="amount">The amount of non base currency</param>
        /// <param name="price">The price to place the order at</param>
        /// <returns>A Response object indicating the status of the order</returns>
        public ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal amount, decimal price)
        {
            var currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            decimal proposedAmount = side == OrderSide.Buy ? amount * price : amount;
            var proposal = new TradeProposal(new Balance(currency, proposedAmount, 0));
            ResponseObject<OrderUpdate> query = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            bool tradeSucces = _allocationManager.QueueTrade(proposal, () =>
            {
                query = RetryMethod(() => _implementation.PlaceLimitOrder(pair, side, amount, price));

                if (!query.Success)
                {
                    return null;
                }

                TradeExecution exec;
                if (side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(currency, amount * price, 0),
                        new Balance(currency, 0, amount * price));
                }
                else
                {
                    exec = new TradeExecution(
                        new Balance(currency, amount, 0),
                        new Balance(currency, 0, amount));
                }

                return exec;
            });
            return tradeSucces ? query : new ResponseObject<OrderUpdate>(ResponseCode.Error);
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="pair">trading pair in which the order is found</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public ResponseObject CancelOrder(TradingPair pair, long orderId)
        {
            return _implementation.CancelOrder(pair, orderId);
        }

        private decimal GetBuyAmountEstimate(TradingPair pair, decimal baseAmount)
        {
            var query = _dataProvider.GetCurrentPriceTopAsk(pair);
            if (!query.Success)
            {
                _logger.LogWarning(query.ToString());
                return 0.0M;
            }

            return baseAmount / query.Data;
        }

        private ResponseObject<T> RetryMethod<T>(Func<ResponseObject<T>> method)
        {
            int retries = 0;
            for (int i = 0; i < 5; i++)
            {
                var result = method();
                if (result.Success)
                {
                    return result;
                }

                _logger.LogWarning($"{result.Message} - attempt {retries}/5");
            }

            return new ResponseObject<T>(ResponseCode.Error);
        }

        private void UpdateAllocation(OrderUpdate order)
        {
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

            _allocationManager.UpdateAllocation(exec);
            UpdateObservers(order);
        }
    }
}
