using System;
using System.Collections.Generic;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
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
        private readonly Dictionary<long, OrderUpdate> _openOrders;

        /// <summary>
        /// Initializes a new instance of the <see cref="TradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="implementation">The implementation to delegate calls to.</param>
        /// <param name="dataProvider">The data provider to manager certain orders with.</param>
        /// <param name="allocationManager">The allocation manager to verify orders.</param>
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
            _openOrders = new Dictionary<long, OrderUpdate>();
            _implementation.Subscribe(new ConfigurableObserver<OrderUpdate>(
                HandleOrderUpdate,
                () => { },
                e => { }));
        }

        /// <summary>
        /// Gets or sets the current ID of the trade under which an order will be placed.
        /// </summary>
        private long TradeId { get; set; }

        /// <summary>
        /// Increment the trade ID, handled by the StateManager.
        /// </summary>
        public void IncrementTradeId() => TradeId++;

        /// <summary>
        /// Gets the portfolio associated with an algorithm.
        /// </summary>
        /// <returns>Response object indicating success or not.</returns>
        public Portfolio GetPortfolio()
        {
            return _allocationManager.GetAllFunds();
        }

        /// <summary>
        /// Place a buy market order using the full allocation.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <returns>ResponseObject with an OrderUpdate.</returns>
        public OrderUpdate ExecuteFullMarketOrderBuy(TradingPair pair)
        {
            Guard.Argument(pair).NotNull();
            var currency = pair.Right;
            var balance = _allocationManager.GetAvailableFunds(currency);
            var quantity = GetBuyQuantityEstimate(pair, balance.Free);
            return ExecuteMarketOrderBuy(pair, quantity);
        }

        /// <summary>
        /// Place a sell market order using the full allocation.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <returns>ResponseObject with an OrderUpdate.</returns>
        public OrderUpdate ExecuteFullMarketOrderSell(TradingPair pair)
        {
            Guard.Argument(pair).NotNull();
            var currency = pair.Left;
            var balance = _allocationManager.GetAvailableFunds(currency);
            return ExecuteMarketOrderSell(pair, balance.Free);
        }

        /// <summary>
        /// Place a buy market order given a non base quantity.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="quantity">Quantity of non base currency.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate ExecuteMarketOrderBuy(TradingPair pair, decimal quantity)
        {
            Guard.Argument(quantity).NotNegative();
            Guard.Argument(pair).NotNull(nameof(pair));
            var currency = pair.Right;
            var priceEstimate = _dataProvider.GetCurrentPriceTopAsk(pair);
            var proposal = new TradeProposal(pair, new Balance(currency, quantity * priceEstimate, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                var order = HelperMethods.RetryMethod(
                    () => _implementation.ExecuteMarketOrder(pair, OrderSide.Buy, quantity, TradeId), _logger).Data;
                return WaitForOrderConfirmation(order.OrderId);
            });

            if (!result.Success)
            {
                throw new OrderRefusedException(result.Message);
            }

            return result.Data;
        }

        /// <summary>
        /// Place a sell market order given a non base quantity.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="quantity">Quantity of non base currency.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate ExecuteMarketOrderSell(TradingPair pair, decimal quantity)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(quantity).NotNegative();
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                var order = HelperMethods.RetryMethod(
                    () => _implementation.ExecuteMarketOrder(pair, OrderSide.Sell, proposal.From.Free, TradeId), _logger).Data;
                return WaitForOrderConfirmation(order.OrderId);
            });

            if (!result.Success)
            {
                throw new OrderRefusedException(result.Message);
            }

            return result.Data;
        }

        /// <summary>
        /// Place a sell limit order given a non base quantity and target price.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="quantity">Quantity of non base currency to trade with.</param>
        /// <param name="price">SetPrice to set order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceLimitOrderBuy(TradingPair pair, decimal quantity, decimal price)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(quantity).NotNegative();
            Guard.Argument(price).NotNegative();
            var currency = pair.Right;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity * price, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                return HelperMethods.RetryMethod(
                    () => _implementation.PlaceLimitOrder(
                        pair,
                        OrderSide.Buy,
                        pair.RoundToTradable(quantity),
                        pair.RoundToPriceable(price),
                        TradeId), _logger).Data;
            });

            if (result.Success)
            {
                _openOrders[result.Data.OrderId] = result.Data;
                return result.Data;
            }

            throw new OrderRefusedException();
        }

        /// <summary>
        /// Place a buy limit order given a non base quantity and a target price.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="quantity">Quantity of non base currency to trade with.</param>
        /// <param name="price">Price to set order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceLimitOrderSell(TradingPair pair, decimal quantity, decimal price)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            Guard.Argument(quantity).NotNegative();
            Guard.Argument(price).NotNegative();
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                return HelperMethods.RetryMethod(
                    () => _implementation.PlaceLimitOrder(
                        pair,
                        OrderSide.Sell,
                        pair.RoundToTradable(quantity),
                        pair.RoundToPriceable(price),
                        TradeId), _logger).Data;
            });

            if (result.Success)
            {
                _openOrders[result.Data.OrderId] = result.Data;
                return result.Data;
            }

            throw new OrderRefusedException(result.Message);
        }

        /// <summary>
        /// Place a sell limit order with the full allocation.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="price">Price to set order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceFullLimitOrderSell(TradingPair pair, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(price).NotNegative();
            var currency = pair.Left;
            var quantity = _allocationManager.GetAvailableFunds(currency).Free;
            return PlaceLimitOrderSell(pair, quantity, price);
        }

        /// <summary>
        /// Place a buy limit order with the full allocation.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="price">Price to set the order at.</param>
        /// <returns>ResponseObject containing and OrderUpdate.</returns>
        public OrderUpdate PlaceFullLimitOrderBuy(TradingPair pair, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(price).NotNegative();
            var currency = pair.Right;
            var quantity = _allocationManager.GetAvailableFunds(currency).Free;
            decimal estimation = quantity / price;
            return PlaceLimitOrderBuy(pair, estimation, price);
        }

        /// <summary>
        /// Place a sell stoploss order.
        /// </summary>
        /// <param name="pair">TradingPair to consider.</param>
        /// <param name="quantity">Quantity of non base currency.</param>
        /// <param name="price">Price to set the order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceStoplossSell(TradingPair pair, decimal quantity, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(quantity).NotNegative();
            Guard.Argument(price).NotNegative();
            var currency = pair.Left;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                return HelperMethods.RetryMethod(
                    () => _implementation.PlaceStoplossOrder(pair, OrderSide.Sell, quantity, price, TradeId),
                    _logger).Data;
            });

            if (result.Success)
            {
                _openOrders[result.Data.OrderId] = result.Data;
                return result.Data;
            }

            throw new OrderRefusedException(result.Message);
        }

        /// <summary>
        /// Place a buy stoploss order.
        /// </summary>
        /// <param name="pair">TradingPair.</param>
        /// <param name="quantity">Quantity of none base currency to trade with.</param>
        /// <param name="price">Price to set the order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceStoplossBuy(TradingPair pair, decimal quantity, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(price).NotNegative();
            var currency = pair.Right;
            var proposal = new TradeProposal(pair, new Balance(currency, quantity * price, 0));

            var result = _allocationManager.QueueTrade(proposal, () =>
            {
                return HelperMethods.RetryMethod(
                    () => _implementation.PlaceStoplossOrder(pair, OrderSide.Buy, quantity, price, TradeId),
                    _logger).Data;
            });

            if (result.Success)
            {
                _openOrders[result.Data.OrderId] = result.Data;
                return result.Data;
            }

            throw new OrderRefusedException();
        }

        /// <summary>
        /// Place a sell stoploss order with the full allocation.
        /// </summary>
        /// <param name="pair">Trading pair.</param>
        /// <param name="price">Price to set the order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceFullStoplossSell(TradingPair pair, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(price).NotNegative();
            var currency = pair.Left;
            decimal quantity = _allocationManager.GetAvailableFunds(currency).Free;
            return PlaceStoplossSell(pair, quantity, price);
        }

        /// <summary>
        /// Place a buy stoploss order with the full allocation.
        /// </summary>
        /// <param name="pair">Trading pair.</param>
        /// <param name="price">Price to set the order at.</param>
        /// <returns>ResponseObject containing an OrderUpdate.</returns>
        public OrderUpdate PlaceFullStoplossBuy(TradingPair pair, decimal price)
        {
            Guard.Argument(pair).NotNull();
            Guard.Argument(price).NotNegative();
            var currency = pair.Right;
            decimal quantity = _allocationManager.GetAvailableFunds(currency).Free;
            return PlaceStoplossBuy(pair, quantity, price);
        }

        /// <summary>
        /// Cancels order.
        /// </summary>
        /// <param name="order">The order to cancel.</param>
        /// <returns>boolean indicating success.</returns>
        public bool CancelOrder(OrderUpdate order)
        {
            Guard.Argument(order).NotNull(nameof(order));
            var query = HelperMethods.RetryMethod(
                () => _implementation.CancelOrder(order.Pair, order.OrderId),
                _logger);

            return query.Success;
        }

        /// <summary>
        /// Get the info of an order.
        /// </summary>
        /// <param name="pair">The TradingPair to consider.</param>
        /// <param name="orderId">The order id related.</param>
        /// <returns>OrderUpdate object containing the state of the order.</returns>
        public OrderUpdate GetOrderInfo(TradingPair pair, long orderId)
        {
            Guard.Argument(pair).NotNull(nameof(pair));
            var query = HelperMethods.RetryMethod(() => _implementation.GetOrderInfo(orderId), _logger);
            if (query.Success)
            {
                return query.Data;
            }

            throw new ExchangeConnectionException(query.Message);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the TradingProvider.
        /// </summary>
        /// <param name="disposing">Actually dispose it.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var order in _openOrders.Values)
                {
                    _logger.LogCritical($"Cancelling order {order.OrderId} ({order.Pair})");
                    if (CancelOrder(order))
                    {
                        _logger.LogWarning($"Order {order.OrderId} could not be cancelled");
                    }
                    else
                    {
                        _logger.LogCritical($"Order {order.OrderId} successfully cancelled");
                    }
                }
            }
        }

        private decimal GetBuyQuantityEstimate(TradingPair pair, decimal baseQuantity)
            => baseQuantity / _dataProvider.GetCurrentPriceTopAsk(pair);

        private void HandleOrderUpdate(OrderUpdate order)
        {
            if (!_openOrders.ContainsKey(order.OrderId) && order.OrderType != OrderUpdate.OrderTypes.Market)
            {
                _logger.LogWarning($"Observed order {order.OrderId} as {order.Status} but the order was not tracked");
                return;
            }

            UpdateAllocation(order);
            UpdateOpenOrders(order);

            // pass the order on to the subscribers
            UpdateObservers(order);
        }

        private void UpdateAllocation(OrderUpdate order)
        {
            // Skip Market orders, there are filled at execution time.
            if (order.OrderType == OrderUpdate.OrderTypes.Market)
            {
                return;
            }

            // Skip new orders, their allocation is already processed
            if (order.Status == OrderUpdate.OrderStatus.New)
            {
                return;
            }

            var exec = TradeExecution.FromOrder(order);
            _allocationManager.UpdateAllocation(exec);
        }

        private void UpdateOpenOrders(OrderUpdate order)
        {
            if (order.Finalized)
            {
                _openOrders.Remove(order.OrderId);
            }
        }

        private OrderUpdate WaitForOrderConfirmation(long orderId)
        {
            var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (true)
            {
                var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (now - start > 10000)
                {
                    throw new ExchangeTimeoutException($"Order {orderId} was not filled within 10 seconds.");
                }

                var query = _implementation.GetOrderInfo(orderId);
                if (query.Success)
                {
                    return query.Data;
                }
            }
        }
    }
}
