using System;
using System.Collections.Generic;
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
    internal class TradingProvider : Observable<OrderUpdate>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly AbstractTradingProvider _implementation;
        private readonly WeakAllocationManager _allocationManager;
        private readonly DataProvider _dataProvider;
        private readonly List<OrderUpdate> _openOrders;

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
            _openOrders = new List<OrderUpdate>();
            _implementation.Subscribe(new ConfigurableObserver<OrderUpdate>(
                UpdateAllocation,
                () => { },
                e => { }));
        }

        /// <summary>
        /// Gets or sets the current ID of the trade under which order will be placed.
        /// </summary>
        private long TradeId { get; set; }

        /// <summary>
        /// Increment the trade ID, handled by the StateManager
        /// </summary>
        public void IncrementTradeId()
        {
            TradeId++;
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
                    _implementation.PlaceMarketOrder(pair, OrderSide.Buy, quantity, TradeId));
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
                    _implementation.PlaceMarketOrder(pair, OrderSide.Sell, proposal.From.Free, TradeId));
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
                result = RetryMethod(() => _implementation.PlaceLimitOrder(pair, OrderSide.Buy, quantity, price, TradeId));
                return result.Success
                    ? new TradeExecution(proposal.From, new Balance(currency, 0, quantity * price))
                    : null;
            });
            if (tradeSucces)
            {
                _openOrders.Add(result.Data);
                return result;
            }

            return ResponseCommon.OrderRefused;
        }

        /// <summary>
        /// Place a buy limit order given a non base quantity and a target price
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="quantity">Quantity of non base currency to trade with</param>
        /// <param name="price">Price to set order at</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceLimitOrderSell(TradingPair pair, decimal quantity, decimal price)
        {
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            ResponseObject<OrderUpdate> result = new ResponseObject<OrderUpdate>(ResponseCode.Error);
            bool tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                result = RetryMethod(() => _implementation.PlaceLimitOrder(pair, OrderSide.Sell, quantity, price, TradeId));
                return result.Success
                    ? new TradeExecution(proposal.From, new Balance(currency, 0, quantity))
                    : null;
            });

            if (tradeSuccess)
            {
                _openOrders.Add(result.Data);
                return result;
            }

            return ResponseCommon.OrderRefused;
        }

        /// <summary>
        /// Place a sell limit order with the full allocation
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="price">Price to set order at</param>
        /// <returns>ResponseObject containing an OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceFullLimitOrderSell(TradingPair pair, decimal price)
        {
            var currency = pair.Left;
            var quantity = _allocationManager.GetAvailableFunds(currency).Free;
            return PlaceLimitOrderSell(pair, quantity, price);
        }

        /// <summary>
        /// Place a buy limit order with the full allocation
        /// </summary>
        /// <param name="pair">TradingPair to consider</param>
        /// <param name="price">Price to set the order at</param>
        /// <returns>ResponseObject containing and OrderUpdate</returns>
        public ResponseObject<OrderUpdate> PlaceFullLimitOrderBuy(TradingPair pair, decimal price)
        {
            var currency = pair.Right;
            var quantity = _allocationManager.GetAvailableFunds(currency).Free;
            return PlaceLimitOrderBuy(pair, quantity, price);
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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the TradingProvider
        /// </summary>
        /// <param name="disposing">actually dispose it</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var order in _openOrders)
                {
                    _logger.LogWarning($"Cancelling order {order.Pair}");
                    CancelOrder(order.Pair, order.OrderId);
                }
            }
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
