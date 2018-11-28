using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;

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
        public Portfolio GetPortfolio()
        {
            return _allocationManager.GetAllFunds();
        }

        /// <summary>
        /// Place a buy market order using the full allocation
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <returns>ResponseObject with an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceFullMarketOrderBuy(TradingPair pair)
        {
            var currency = pair.Right;
            var balance = _allocationManager.GetAvailableFunds(currency);
            var quantity = GetBuyQuantityEstimate(pair, balance.Free);
            return PlaceMarketOrderBuy(pair, quantity);
        }

        /// <summary>
        /// Place a sell market order using the full allocation
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <returns>ResponseObject with an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceFullMarketOrderSell(TradingPair pair)
        {
            var currency = pair.Left;
            var balance = _allocationManager.GetAvailableFunds(currency);
            return PlaceMarketOrderSell(pair, balance.Free);
        }

        /// <summary>
        /// Place a buy market order given a non base quantity.
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="quantity">Quantity of non base currency</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceMarketOrderBuy(TradingPair pair, decimal quantity)
        {
            var currency = pair.Right;
            var priceEstimate = _dataProvider.GetCurrentPriceTopAsk(pair).Data;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity * priceEstimate, 0));

            ResponseObject<OrderUpdate> result = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            var tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                result = RetryMethod(() =>
                    _implementation.PlaceMarketOrder(pair, OrderSide.Buy, quantity));
                return result.Success
                    ? new TradeExecution(proposal.From, new Balance(pair.Left, result.Data.SetQuantity, 0.0M))
                    : null;
            });
            return tradeSuccess
                ? result
                : ResponseCommon.OrderRefused;
        }

        /// <summary>
        /// Place a sell market order given a non base quantity
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="quantity">Quantity of non base currency</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceMarketOrderSell(TradingPair pair, decimal quantity)
        {
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            ResponseObject<OrderUpdate> result = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            var tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                result = RetryMethod(() =>
                    _implementation.PlaceMarketOrder(pair, OrderSide.Sell, proposal.From.Free));
                if (result.Success)
                {
                    // Correct gained quantity using a price estimate
                    decimal priceEstimate = _dataProvider.GetCurrentPriceTopBid(pair).Data;
                    return new TradeExecution(
                        proposal.From,
                        new Balance(pair.Right, result.Data.SetQuantity * priceEstimate, 0));
                }

                return null;
            });
            return tradeSuccess
                ? result
                : ResponseCommon.OrderRefused;
        }

        /// <summary>
        /// Place a selll limit order given a non base quantity and target price
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="quantity">Quantity of non base currency to trade with</param>
        /// <param name="price">SetPrice to set order at</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceLimitOrderBuy(TradingPair pair, decimal quantity, decimal price)
        {
            var currency = pair.Right;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity * price, 0));

            ResponseObject<OrderUpdate> result = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            bool tradeSucces = _allocationManager.QueueTrade(proposal, () =>
            {
                result = RetryMethod(() => _implementation.PlaceLimitOrder(pair, OrderSide.Buy, quantity, price));
                return result.Success
                    ? new TradeExecution(proposal.From, new Balance(currency, 0, quantity * price))
                    : null;
            });
            return tradeSucces ? result : ResponseCommon.OrderRefused;
        }

        /// <summary>
        /// Place a buy limit order given a non base quantity and a target price
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="quantity">Quantity of non base currency to trade with</param>
        /// <param name="price">SetPrice to set order at</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceLimitOrderSell(TradingPair pair, decimal quantity, decimal price)
        {
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            ResponseObject<OrderUpdate> result = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            bool tradeSucces = _allocationManager.QueueTrade(proposal, () =>
            {
                result = RetryMethod(() => _implementation.PlaceLimitOrder(pair, OrderSide.Sell, quantity, price));
                return result.Success
                    ? new TradeExecution(proposal.From, new Balance(currency, 0, quantity))
                    : null;
            });
            return tradeSucces ? result : ResponseCommon.OrderRefused;
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
                    new Balance(order.Pair.Right, order.SetQuantity * order.AverageFilledPrice, 0));
            }

            _allocationManager.UpdateAllocation(exec);
            UpdateObservers(order);
        }
    }
}
