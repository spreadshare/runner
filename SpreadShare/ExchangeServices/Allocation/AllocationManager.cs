using System;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.Utilities;

namespace SpreadShare.ExchangeServices.Allocation
{
    /// <summary>
    /// This class provides allocation management for multiple algorithms.
    /// </summary>
    internal class AllocationManager : Observable<Portfolio>, IAllocationManager
    {
        private readonly ILogger _logger;
        private readonly IPortfolioFetcherService _portfolioFetcherService;
        private readonly DatabaseContext _databaseContext;
        private Portfolio _allocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logger instance.</param>
        /// <param name="portfolioFetcherService">Provides portfolio fetching capabilities.</param>
        /// <param name="db">Used to check available allocation.</param>
        public AllocationManager(ILoggerFactory loggerFactory, IPortfolioFetcherService portfolioFetcherService, DatabaseContext db)
        {
            _logger = loggerFactory.CreateLogger<AllocationManager>();
            _portfolioFetcherService = portfolioFetcherService;
            _databaseContext = db;
        }

        /// <inheritdoc />
        public void SetInitialConfiguration(Portfolio initialAllocation)
        {
            if (_allocation != null && Program.CommandLineArgs.Trading)
            {
                _logger.LogError("Initial allocation is being set, but had been initialized before..");
                throw new InvalidOperationException("Attempt to reconfigure allocation");
            }

            // Get the assets from the exchange
            var remoteQuery = HelperMethods.RetryMethod(_portfolioFetcherService.GetPortfolio, _logger);
            if (!remoteQuery.Success)
            {
                throw new ExchangeConnectionException(remoteQuery.Message);
            }

            // Get the already allocated resources from the active sessions
            var allocated = Portfolio.Empty;
            if (Program.CommandLineArgs.Trading && !Program.CommandLineArgs.SkipDatabase)
            {
                try
                {
                    var allocatedPortfolios = _databaseContext.Sessions.Where(x => x.Active).Select(x => x.Allocation);
                    foreach (var allocatedPortfolio in allocatedPortfolios)
                    {
                        allocated += allocatedPortfolio;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogWarning($"Database was not available, cannot verify if requested allocation is available.");
                }
            }

            // Available allocation are the assets on the exchange, minus the already allocated resources.
            var available = remoteQuery.Data - allocated;

            if (!initialAllocation.ContainedIn(available))
            {
                _logger.LogError($"The requested allocation:\n {JsonConvert.SerializeObject(initialAllocation, Formatting.Indented)}\n"
                                 + $"was not contained in the available assets:\n {JsonConvert.SerializeObject(available, Formatting.Indented)}");
                throw new AllocationUnavailableException();
            }

            _allocation = initialAllocation;
            if (Program.CommandLineArgs.Trading)
            {
                _logger.LogInformation($"Configured AllocationManager with {JsonConvert.SerializeObject(_allocation)}");
            }

            UpdateObservers(_allocation);
        }

        /// <inheritdoc />
        public Portfolio GetAllFunds()
        {
            Guard.Argument(_allocation).NotNull("Initialise allocations first");
            return _allocation;
        }

        /// <inheritdoc />
        public Balance GetAvailableFunds(Currency currency)
        {
            Guard.Argument(_allocation).NotNull("Initialise allocations first");
            return _allocation.GetAllocation(currency);
        }

        /// <inheritdoc />
        public void UpdateAllocation(TradeExecution exec)
        {
            if (Program.CommandLineArgs.Trading)
            {
                _logger.LogInformation($"Allocation Update: {JsonConvert.SerializeObject(exec, Formatting.Indented)}");
            }

            _allocation.UpdateAllocation(exec);
            UpdateObservers(_allocation);
        }

         /// <summary>
        /// Queue a trade based on a proposal, the callback must return the trade execution
        /// which will be used to update the allocation.
        /// </summary>
        /// <param name="p">TradeProposal to be verified.</param>
        /// <param name="tradeCallback">Trade callback to be executed if verification was successful.</param>
        /// <returns>Boolean indicating successful execution of the callback.</returns>
        public virtual ResponseObject<OrderUpdate> QueueTrade(TradeProposal p, Func<OrderUpdate> tradeCallback)
        {
            Guard.Argument(_allocation).NotNull("Initialise allocations first");
            Guard.Argument(p).NotNull();

            var alloc = GetAvailableFunds(p.From.Symbol);
            if (alloc.Free < p.From.Free || alloc.Locked < p.From.Locked)
            {
                _logger.LogCritical($"Got trade proposal for ({p.From}, but allocation " +
                                    $"showed only ({alloc}) was available\n" +
                                    "Trade will not be executed.");
                return ResponseObject.OrderRefused;
            }

            // Let the provider execute the trade and save the execution report
            var order = tradeCallback();

            // TradingProvider can give a null execution report when it decides not to trade.
            // if this happens the portfolio will be checked against the remote using an 'empty' or 'monoid' trade execution.
            if (order is null)
            {
                _logger.LogWarning("TradingProvider implementation returned a null OrderUpdate");
                return ResponseObject.OrderPlacementFailed("Implementation returned a null OrderUpdate");
            }

            TradeExecution exec = TradeExecution.FromOrder(order);

            // Update the local information
            UpdateAllocation(exec);

            return new ResponseObject<OrderUpdate>(order);
        }
    }
}
