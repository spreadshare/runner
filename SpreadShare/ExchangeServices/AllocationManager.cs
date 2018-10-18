using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices
{
    /// <summary>
    /// This class provides allocation management for multiple strategies.
    /// </summary>
    internal class AllocationManager
    {
        // TODO: Is the DustThreshold different per currency?
        private const decimal DustThreshold = 0.01M;

        private readonly ILogger _logger;
        private Dictionary<Type, Dictionary<Currency, decimal>> _allocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logger instance</param>
        public AllocationManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AllocationManager>();
            _allocations = null;
        }

        /// <summary>
        /// Sets initial configuration of allocations per algorithm.
        /// </summary>
        /// <param name="allocations">Initial set of allocations</param>
        public void SetInitialConfiguration(Dictionary<Type, Dictionary<Currency, decimal>> allocations)
        {
            if (_allocations != null)
            {
                throw new Exception("Allocation Manager already configured");
            }

            _logger.LogTrace("Configured AllocationManager");
            _allocations = allocations;
        }

        /// <summary>
        /// Check if algorithm has enough of certain currency
        /// </summary>
        /// <param name="algorithm">The algorithm that wants to execute a trade</param>
        /// <param name="currency">The currency to be sold</param>
        /// <param name="fundsToTrade">The amount to be sold of given currency</param>
        /// <returns>Returns if enough funds are present to execute the trade</returns>
        public bool CheckFunds(Type algorithm, Currency currency, decimal fundsToTrade)
        {
            // Check if algorithm is allocated
            if (!_allocations.ContainsKey(algorithm))
            {
                _logger.LogTrace($"CheckFunds: Algorithm {algorithm} not available.");
                return false;
            }

            // Check if algorithm's portfolio contains currency
            if (_allocations[algorithm].ContainsKey(currency))
            {
                _logger.LogTrace($"CheckFunds: Currency {currency} not available for Algorithm {algorithm}.");
                return false;
            }

            // Check if algorithm's portfolio has enough of the currency
            return _allocations[algorithm][currency] >= fundsToTrade;
        }

        /// <summary>
        /// Get available funds for a given algorithm and currency.
        /// </summary>
        /// <param name="algorithm">Algorithm to get funds for</param>
        /// <param name="currency">Currency to get funds for</param>
        /// <returns>Available funds or -1 if not available</returns>
        public decimal GetAvailableFunds(Type algorithm, Currency currency)
        {
            // Check if algorithm is allocated
            if (!_allocations.ContainsKey(algorithm))
            {
                _logger.LogTrace($"GetAvailableFunds: Algorithm {algorithm} not available.");
                return -1;
            }

            // Check if algorithm's portfolio contains currency
            if (_allocations[algorithm].ContainsKey(currency))
            {
                _logger.LogTrace($"GetAvailableFunds: Currency {currency} not available for Algorithm {algorithm}.");
                return -1;
            }

            // Check if algorithm's portfolio has enough of the currency
            if (_allocations[algorithm][currency] < DustThreshold)
            {
                _logger.LogTrace($"GetAvailableFunds: Not enough funds availble for Currency {currency} for Algorithm {algorithm}.");
                return -1;
            }

            return _allocations[algorithm][currency];
        }

        /// <summary>
        /// Update portfolio after trade.
        /// </summary>
        private void UpdatePortfolio()
        {
            throw new NotImplementedException("Waiting for PortfolioFetcherService to be implemented");
        }
    }
}
