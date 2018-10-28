using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// This class provides allocation management for multiple algorithms.
    /// </summary>
    internal class AllocationManager : ITradeObserver
    {
        // TODO: Is the DustThreshold different per currency?
        private const decimal DustThreshold = 0.01M;

        private readonly ILogger _logger;
        private readonly IPortfolioFetcherService _portfolioFetcherService;
        private Dictionary<Exchange, Dictionary<Type, Assets>> _allocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logger instance</param>
        /// <param name="portfolioFetcherService">Provides portfolio fetching capabilities</param>
        public AllocationManager(ILoggerFactory loggerFactory, IPortfolioFetcherService portfolioFetcherService)
        {
            _logger = loggerFactory.CreateLogger<AllocationManager>();
            _portfolioFetcherService = portfolioFetcherService;
            _allocations = null;
        }

        /// <summary>
        /// Sets initial configuration of allocations per algorithm.
        /// </summary>
        /// <param name="initialAllocations">Initial set of allocations</param>
        public void SetInitialConfiguration(Dictionary<Exchange, Dictionary<Type, decimal>> initialAllocations)
        {
            // Make sure AllocationManager is not already configured
            if (_allocations != null)
            {
                throw new Exception("AllocationManager already configured");
            }

            // Initialise _allocations
            _allocations = new Dictionary<Exchange, Dictionary<Type, Assets>>();

            // Get Assets for all configured exchanges
            Dictionary<Exchange, Assets> balances = new Dictionary<Exchange, Assets>();
            foreach (var exchangeEntry in initialAllocations)
            {
                balances.Add(exchangeEntry.Key, _portfolioFetcherService.GetPortfolio(exchangeEntry.Key).Data);
            }

            // Loop over exchanges
            foreach (var exchangeEntry in balances)
            {
                // Add exchange to _allocations
                _allocations.Add(exchangeEntry.Key, new Dictionary<Type, Assets>());

                // Loop over configured algorithms
                foreach (var algorithmType in initialAllocations[exchangeEntry.Key].Keys)
                {
                    // Copy scaled down assets to _allocations
                    _allocations[exchangeEntry.Key][algorithmType] =
                        exchangeEntry.Value.DuplicateWithScale(
                            initialAllocations[exchangeEntry.Key][algorithmType]);
                }
            }

            _logger.LogTrace("Configured AllocationManager");
        }

        /// <summary>
        /// Check if algorithm has enough of certain currency
        /// </summary>
        /// <param name="exchange">Exchange to trade on</param>
        /// <param name="algorithm">The algorithm that wants to execute a trade</param>
        /// <param name="currency">The currency to be sold</param>
        /// <param name="fundsToTrade">The amount to be sold of given currency</param>
        /// <returns>Returns if enough funds are present to execute the trade</returns>
        public bool CheckFunds(Exchange exchange, Type algorithm, Currency currency, decimal fundsToTrade)
        {
            // Check if exchange is used
            if (!_allocations.ContainsKey(exchange))
            {
                _logger.LogTrace($"CheckFunds: Exchange {exchange} not available.");
                return false;
            }

            // Check if algorithm is allocated
            if (!_allocations[exchange].ContainsKey(algorithm))
            {
                _logger.LogTrace($"CheckFunds: Algorithm {algorithm} not available.");
                return false;
            }

            return _allocations[exchange][algorithm].GetFreeBalance(currency) >= fundsToTrade;
        }

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="exchange">Exchange to trade on</param>
        /// <param name="algorithm">Algorithm to get funds for</param>
        /// <param name="currency">Currency to get funds for</param>
        /// <returns>Available funds or -1 if not available</returns>
        public decimal GetAvailableFunds(Exchange exchange, Type algorithm, Currency currency)
        {
            // Check if exchange is used
            if (!_allocations.ContainsKey(exchange))
            {
                _logger.LogTrace($"GetAvailableFunds: Algorithm {algorithm} not available.");
                return -1;
            }

            // Check if algorithm is allocated
            if (!_allocations[exchange].ContainsKey(algorithm))
            {
                _logger.LogTrace($"GetAvailableFunds: Algorithm {algorithm} not available.");
                return -1;
            }

            // Check if algorithm's portfolio has enough of the currency
            if (_allocations[exchange][algorithm].GetFreeBalance(currency) < DustThreshold)
            {
                _logger.LogTrace($"GetAvailableFunds: Not enough funds availble for Currency {currency} for Algorithm {algorithm}.");
                return -1;
            }

            return _allocations[exchange][algorithm].GetFreeBalance(currency);
        }

        /// <summary>
        /// Get weakened version of allocation manager for the trading provider.
        /// </summary>
        /// <returns>Weakened version of allocation manager</returns>
        public WeakAllocationManager GetWeakAllocationManager() => new WeakAllocationManager(this);

        /// <inheritdoc />
        public void Update(Type algorithm, IExchangeSpecification exchangeSpecification)
            => UpdatePortfolio(algorithm, exchangeSpecification);

        /// <summary>
        /// Update portfolio after trade.
        /// </summary>
        /// <param name="algorithm">Algorithm that has traded</param>
        /// <param name="exchangeSpecification">Specifies which exchange is used</param>
        private void UpdatePortfolio(Type algorithm, IExchangeSpecification exchangeSpecification)
        {
            // TODO: Update allocation
            algorithm = null;
            _portfolioFetcherService.GetPortfolio(exchangeSpecification);
        }
    }
}
