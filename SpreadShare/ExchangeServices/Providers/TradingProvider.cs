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
        /// Places market order with the full quantity of given pair
        /// </summary>
        /// <param name="pair">trading pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public ResponseObject<OrderUpdate> PlaceFullMarketOrder(TradingPair pair, OrderSide side)
        {
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            Balance balance = _allocationManager.GetAvailableFunds(currency);
            var proposal = new TradeProposal(new Balance(currency, balance.Free, 0.0M));

            ResponseObject<OrderUpdate> query = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            var tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                query = RetryMethod(() =>
                {
                    decimal tradeQuantity = side == OrderSide.Buy
                        ? GetBuyQuantityEstimate(pair, proposal.From.Free)
                        : proposal.From.Free;
                    return _implementation.PlaceFullMarketOrder(pair, side, tradeQuantity);
                });

                if (!query.Success)
                {
                    return null;
                }

                // Report the trade with the actual quantity as communicated by the exchange.
                TradeExecution exec;
                if (side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(pair.Right, proposal.From.Free, 0.0M),
                        new Balance(pair.Left, query.Data.SetQuantity, 0.0M));
                }
                else
                {
                    decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
                    exec = new TradeExecution(
                        new Balance(pair.Left, proposal.From.Free, 0.0M),
                        new Balance(pair.Right, query.Data.SetQuantity * priceEstimate, 0.0M));
                }

                return exec;
            });

            if (tradeSuccess)
            {
                return query;
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order was refused by AllocationManager");
        }

        public ResponseObject<OrderUpdate> PlaceMarketOrder(TradingPair pair, OrderSide side, decimal quantity)
        {
            var currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            decimal correctedAmount = side == OrderSide.Buy
                ? quantity *
                  _dataProvider.GetCurrentPriceLastTrade(pair).Data
                : quantity;
            var proposal = new TradeProposal(new Balance(currency, correctedAmount, 0));
            ResponseObject<OrderUpdate> query = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            var tradeSuccess = _allocationManager.QueueTrade(proposal,
                () =>
                {
          
                    query = RetryMethod(() => _implementation.PlaceFullMarketOrder(pair, side, quantity));
                    if (!query.Success)
                    {
                        return null;
                    }

                    // Report the trade with the actual quantity as communicated by the exchange.
                    TradeExecution exec;
                    if (side == OrderSide.Buy)
                    {
                        exec = new TradeExecution(
                            new Balance(pair.Right, proposal.From.Free, 0.0M),
                            new Balance(pair.Left, query.Data.SetQuantity, 0.0M));
                    }
                    else
                    {
                        decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
                        exec = new TradeExecution(
                            new Balance(pair.Left, proposal.From.Free, 0.0M),
                            new Balance(pair.Right, query.Data.SetQuantity * priceEstimate, 0.0M));
                    }

                    return exec;
                });
            if (tradeSuccess)
            {
                return query;
            }

            return new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order was refused by AllocationManager");
        }

        /// <summary>
        /// Place a limit order at a certain price
        /// </summary>
        /// <param name="pair">Trading Pair</param>
        /// <param name="side">Buy or sell</param>
        /// <param name="quantity">The quantity of non base currency</param>
        /// <param name="price">The price to place the order at</param>
        /// <returns>A Response object indicating the status of the order</returns>
        public ResponseObject<OrderUpdate> PlaceLimitOrder(TradingPair pair, OrderSide side, decimal quantity, decimal price)
        {
            var currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            decimal proposedQuantity = side == OrderSide.Sell ? quantity * price : quantity;
            var proposal = new TradeProposal(new Balance(currency, proposedQuantity, 0));
            ResponseObject<OrderUpdate> query = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            bool tradeSucces = _allocationManager.QueueTrade(proposal, () =>
            {
                query = RetryMethod(() => _implementation.PlaceLimitOrder(pair, side, quantity, price));

                if (!query.Success)
                {
                    return null;
                }

                TradeExecution exec;
                if (side == OrderSide.Buy)
                {
                    exec = new TradeExecution(
                        new Balance(currency, quantity * price, 0),
                        new Balance(currency, 0, quantity * price));
                }
                else
                {
                    exec = new TradeExecution(
                        new Balance(currency, quantity, 0),
                        new Balance(currency, 0, quantity));
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
            var order = _implementation.GetOrderInfo(pair, orderId).Data;
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

            var query = _implementation.CancelOrder(pair, orderId);
            if (query.Success)
            {
                _allocationManager.UpdateAllocation(exec);
            }

            return query;
        }

        /// <summary>
        /// Get the info of an order
        /// </summary>
        /// <param name="pair">The TradingPair to consider</param>
        /// <param name="orderId">The order id related</param>
        /// <returns>OrderUpdate object containing the state of the order</returns>
        public ResponseObject<OrderUpdate> GetOrderInfo(TradingPair pair, long orderId)
        {
            return _implementation.GetOrderInfo(pair, orderId);
        }

        private decimal GetBuyQuantityEstimate(TradingPair pair, decimal baseQuantity)
        {
            var query = _dataProvider.GetCurrentPriceTopAsk(pair);
            if (!query.Success)
            {
                _logger.LogWarning(query.ToString());
                return 0.0M;
            }

            return baseQuantity / query.Data;
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
                    new Balance(order.Pair.Right, 0, order.SetQuantity * order.SetPrice),
                    new Balance(order.Pair.Left, order.LastFillIncrement, 0));
            }
            else
            {
                exec = new TradeExecution(
                    new Balance(order.Pair.Left, 0, order.LastFillIncrement),
                    new Balance(order.Pair.Right, order.SetQuantity * order.AveragePrice, 0));
            }

            _allocationManager.UpdateAllocation(exec);
            UpdateObservers(order);
        }
    }
}
