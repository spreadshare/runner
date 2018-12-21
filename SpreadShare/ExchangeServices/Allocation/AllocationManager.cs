using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.Models.Trading;

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
            // Make sure AllocationManager is not already configured
            Guard.Argument(_allocations).Null(_ => "Allocation manager is already configured");

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
        public void UpdateAllocation(Exchange exchange, Type algo, TradeExecution exec)
        {
            _allocations[exchange].ApplyTradeExecution(algo, exec);
            UpdatePortfolioUsingRemote(exchange, algo);
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
        public bool QueueTrade(TradeProposal p, Type algorithm, Exchange exchange, Func<TradeExecution> tradeCallback)
        {
            Guard.Argument(_allocations).NotNull("Initialise allocations first");
            Guard.Argument(p).NotNull();

            var alloc = GetAvailableFunds(exchange, algorithm, p.From.Symbol);
            if (alloc.Free < p.From.Free || alloc.Locked < p.From.Locked)
            {
                _logger.LogCritical($"Got trade proposal for ({p.From}, but allocation " +
                                    $"showed only ({alloc}) was available\n" +
                                    "Trade will not be executed.");
                return false;
            }

            // Let the provider execute the trade and save the execution report
            var exec = tradeCallback();

            // TradingProvider can give a null execution report when it decides not to trade.
            // if this happens the portfolio will be checked against the remote using an 'empty' or 'monoid' trade execution.
            if (exec is null)
            {
                _logger.LogWarning("TradeExecution report was null, assuming TradingProvider cancelled proposed trade");

                // Check that the portfolio did not mutate by proceeding with a monodic execution
                exec = new TradeExecution(Balance.Empty(p.From.Symbol), Balance.Empty(p.From.Symbol));
            }

            // Update the local information
            _allocations[exchange].ApplyTradeExecution(algorithm, exec);

           UpdatePortfolioUsingRemote(exchange, algorithm);

           return true;
        }

        private void UpdatePortfolioUsingRemote(Exchange exchange, Type algorithm)
        {
            // Fetch the remote portfolio
            var query = _portfolioFetcherService.GetPortfolio(exchange);
            if (!query.Success)
            {
                _logger.LogWarning("Remote portfolio could not be fetched and the not trade could confirmed, " +
                                   "Assuming local version for now.");
                return;
            }

            var remote = query.Data;
            var local = _allocations[exchange].GetSummedChildren();
            var diff = Portfolio.SubtractedDifferences(remote, local);

            if (diff.Any(x => Math.Abs(x.Free) > DustThreshold || Math.Abs(x.Locked) > DustThreshold))
            {
                _logger.LogWarning("There was a significant discrepancy between the remote and local portfolio, " +
                                   $"Assuming the remote portfolio as truth value whilst blaming {algorithm}.\n" +
                                   $"local portfolio: {local.ToJson()}\n" +
                                   $"remote portfolio: {remote.ToJson()}\n");
            }

            // Compensate discrepancy by blaming and correcting the local allocation for the current algorithm.
            // This is done by adding the differences to the current algorithms allocation.
            var old = _allocations[exchange].GetAlgorithmAllocation(algorithm);
            var diffPortfolio = new Portfolio(diff.ToDictionary(x => x.Symbol, x => x));
            _allocations[exchange].SetAlgorithmAllocation(algorithm, Portfolio.Add(old, diffPortfolio));
        }
    }
}
