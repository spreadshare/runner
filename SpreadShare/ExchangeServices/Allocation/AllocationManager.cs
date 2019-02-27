using System;
using System.Collections.Generic;
using Dawn;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// This class provides allocation management for multiple algorithms.
    /// </summary>
    internal class AllocationManager
    {
        // TODO: Is the DustThreshold different per currency?
        private const decimal DustThreshold = 0.01M;

        private readonly ILogger _logger;
        private readonly IPortfolioFetcherService _portfolioFetcherService;
        private Dictionary<Exchange, TotalPortfolio> _allocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logger instance.</param>
        /// <param name="portfolioFetcherService">Provides portfolio fetching capabilities.</param>
        public AllocationManager(ILoggerFactory loggerFactory, IPortfolioFetcherService portfolioFetcherService)
        {
            _logger = loggerFactory.CreateLogger<AllocationManager>();
            _portfolioFetcherService = portfolioFetcherService;
            _allocations = null;
        }

        /// <summary>
        /// Sets initial configuration of allocations per algorithm.
        /// </summary>
        /// <param name="initialAllocations">Initial set of allocations.</param>
        public void SetInitialConfiguration(Dictionary<Exchange, Dictionary<Type, decimal>> initialAllocations)
        {
            // Initialise _allocations
            _allocations = new Dictionary<Exchange, TotalPortfolio>();

            // Get Assets for all configured exchanges
            Dictionary<Exchange, Portfolio> balances = new Dictionary<Exchange, Portfolio>();
            foreach (var exchangeEntry in initialAllocations)
            {
                balances.Add(exchangeEntry.Key, _portfolioFetcherService.GetPortfolio(exchangeEntry.Key).Data);
            }

            // Loop over exchanges
            foreach (var exchangeEntry in balances)
            {
                // Add exchange to _allocations
                _allocations.Add(exchangeEntry.Key, new TotalPortfolio());

                // Loop over configured algorithms
                foreach (var algorithmType in initialAllocations[exchangeEntry.Key].Keys)
                {
                    // Copy scaled down assets to _allocations
                    _allocations[exchangeEntry.Key].SetAlgorithmAllocation(
                        algorithmType,
                        Portfolio.DuplicateWithScale(
                            exchangeEntry.Value,
                            initialAllocations[exchangeEntry.Key][algorithmType]));
                }
            }

            _logger.LogTrace("Configured AllocationManager");
        }

        /// <summary>
        /// Gives the entire portfolio of a certain algorithm on a certain exchange.
        /// </summary>
        /// <param name="exchange">The exchange in question.</param>
        /// <param name="algorithm">The algorithm in question.</param>
        /// <returns>Portfolio containing all available funds.</returns>
        public Portfolio GetAllFunds(Exchange exchange, Type algorithm)
        {
            Guard.Argument(_allocations).NotNull("Initialise allocations first");

            return _allocations[exchange].GetAlgorithmAllocation(algorithm);
        }

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="exchange">Exchange to trade on.</param>
        /// <param name="algorithm">Algorithm to get funds for.</param>
        /// <param name="currency">Currency to get funds for.</param>
        /// <returns>Available funds or -1 if not available.</returns>
        public Balance GetAvailableFunds(Exchange exchange, Type algorithm, Currency currency)
        {
            Guard.Argument(_allocations).NotNull("Initialise allocations first");

            // Check if exchange is used
            if (!_allocations.ContainsKey(exchange))
            {
                _logger.LogTrace($"GetAvailableFunds: Algorithm {algorithm} not available.");
                return Balance.Empty(currency);
            }

            // Check if algorithm is allocated
            if (!_allocations[exchange].IsAllocated(algorithm))
            {
                _logger.LogTrace($"GetAvailableFunds: Algorithm {algorithm} not available.");
                return Balance.Empty(currency);
            }

            // Check if algorithm's portfolio has enough of the currency
            if (_allocations[exchange].GetAlgorithmAllocation(algorithm).GetAllocation(currency).Free < DustThreshold)
            {
                _logger.LogTrace($"GetAvailableFunds: Not enough funds availble for Currency {currency} for Algorithm {algorithm}.");
                return Balance.Empty(currency);
            }

            return _allocations[exchange].GetAlgorithmAllocation(algorithm).GetAllocation(currency);
        }

        /// <summary>
        /// Updates the allocation of a given algorithm, on a certain exchange given a trade execution.
        /// </summary>
        /// <param name="exchange">The exchange to represent.</param>
        /// <param name="algo">The algorithm to represent.</param>
        /// <param name="exec">The trade execution to process.</param>
        public virtual void UpdateAllocation(Exchange exchange, Type algo, TradeExecution exec)
        {
            _logger.LogInformation($"Allocation Update: {JsonConvert.SerializeObject(exec)}");
            _allocations[exchange].ApplyTradeExecution(algo, exec);
        }

        /// <summary>
        /// Gets a weakened version of allocation manager for the trading provider.
        /// </summary>
        /// <param name="algorithm">The algorithm to represent.</param>
        /// <param name="exchange">The exchange to represent.</param>
        /// <returns>Weakened version of allocation manager.</returns>
        public WeakAllocationManager GetWeakAllocationManager(Type algorithm, Exchange exchange)
            => new WeakAllocationManager(this, algorithm, exchange);

         /// <summary>
        /// Queue a trade based on a proposal, the callback must return the trade execution
        /// which will be used to update the allocation.
        /// </summary>
        /// <param name="p">TradeProposal to be verified.</param>
        /// <param name="algorithm">The algorithm in question.</param>
        /// <param name="exchange">The exchange in question.</param>
        /// <param name="tradeCallback">Trade callback to be executed if verification was successful.</param>
        /// <returns>Boolean indicating successful execution of the callback.</returns>
        public virtual ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Type algorithm, Exchange exchange, Func<OrderUpdate> tradeCallback)
        {
            Guard.Argument(_allocations).NotNull("Initialise allocations first");
            Guard.Argument(p).NotNull();

            var alloc = GetAvailableFunds(exchange, algorithm, p.From.Symbol);
            if (alloc.Free < p.From.Free || alloc.Locked < p.From.Locked)
            {
                _logger.LogCritical($"Got trade proposal for ({p.From}, but allocation " +
                                    $"showed only ({alloc}) was available\n" +
                                    "Trade will not be executed.");
                return ResponseCommon.OrderRefused;
            }

            // Let the provider execute the trade and save the execution report
            var order = tradeCallback();

            // TradingProvider can give a null execution report when it decides not to trade.
            // if this happens the portfolio will be checked against the remote using an 'empty' or 'monoid' trade execution.
            if (order is null)
            {
                _logger.LogWarning("TradingProvider implementation returned a null OrderUpdate");
                return ResponseCommon.OrderPlacementFailed("Implementation returned a null OrderUpdate");
            }

            TradeExecution exec = TradeExecution.FromOrder(order);

            // Update the local information
            UpdateAllocation(exchange, algorithm, exec);

            return new ResponseObject<OrderUpdate>(order);
        }
    }
}
