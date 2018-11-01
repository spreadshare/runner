using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Provides trading capabilities.
    /// </summary>
    internal class TradingProvider
    {
        private readonly ILogger _logger;
        private readonly AbstractTradingProvider _implementation;
        private readonly WeakAllocationManager _allocationManager;
        private readonly DataProvider _dataProvider;
        private readonly Type _algorithm;
        private readonly Exchange _exchange;

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
            WeakAllocationManager allocationManager,
            Type algorithm,
            Exchange exchange)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _implementation = implementation;
            _allocationManager = allocationManager;
            _dataProvider = dataProvider;
            _algorithm = algorithm;
            _exchange = exchange;
        }

        /// <summary>
        /// Gets the portfolio associated with an algorithm
        /// </summary>
        /// <returns>Response object indicating success or not</returns>
        /// TODO: Make method algorithm specific
        public ResponseObject<Portfolio> GetPortfolio()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Places market order with the full amount of given pair
        /// </summary>
        /// <param name="pair">Currency pair to trade with</param>
        /// <param name="side">Whether to buy or sell</param>
        /// <returns>A response object indicating the status of the market order</returns>
        public ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side)
        {
            Currency currency = side == OrderSide.Buy ? pair.Right : pair.Left;
            Balance amount = _allocationManager.GetAvailableFunds(currency);
            var proposal = new TradeProposal(new Balance(currency, amount.Free, 0.0M), _algorithm);

            var tradeSuccess = _allocationManager.QueueTrade(proposal, () =>
            {
                ResponseObject<decimal> query = null;
                decimal tradeAmount = proposal.From.Free;
                for (uint retries = 0; retries < 5; retries++)
                {
                    // Estimate the value that will be obtained from the order when buying.
                    tradeAmount = side == OrderSide.Buy ? GetBuyAmountEstimate(pair, proposal.From.Free) : proposal.From.Free;
                    query = _implementation.PlaceFullMarketOrder(pair, side, tradeAmount);
                    if (query.Success)
                    {
                        break;
                    }

                    _logger.LogWarning(query.ToString());
                }

                if (!query.Success)
                {
                    _logger.LogError($"Trade for {pair} failed after 5 retries");
                    return null;
                }

                // Report the trade with the actual amount as communicated by the exchange.
                // TODO: Is this correct???
                return new TradeExecution(
                    new Balance(pair.Left, proposal.From.Free, 0.0M),
                    new Balance(pair.Right, query.Data, 0.0M),
                    _algorithm);
            });

            return tradeSuccess ? new ResponseObject(ResponseCode.Success) : new ResponseObject(ResponseCode.Error);
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="pair">Currency pair in which the order is found</param>
        /// <param name="orderId">Id of the order</param>
        /// <returns>A response object with the results of the action</returns>
        public ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            return _implementation.CancelOrder(pair, orderId);
        }

        private decimal GetBuyAmountEstimate(CurrencyPair pair, decimal baseAmount)
        {
            var query = _dataProvider.GetCurrentPriceTopAsk(pair);
            if (!query.Success)
            {
                _logger.LogWarning(query.ToString());
                return 0.0M;
            }

            return baseAmount / query.Data;
        }
    }
}
