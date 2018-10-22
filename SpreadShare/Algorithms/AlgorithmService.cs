using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Manages running algorithms.
    /// </summary>
    internal class AlgorithmService : IAlgorithmService
    {
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly AllocationManager _allocationManager;
        private readonly ExchangeFactoryService _exchangeFactoryService;
        private readonly Dictionary<Type, bool> _algorithms;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging capabilities</param>
        /// <param name="settingsService">Provides allocation and algorithm settings</param>
        /// <param name="allocationManager">Provides allocation management</param>
        /// <param name="exchangeFactoryService">Provides containers for algorithms</param>
        public AlgorithmService(
            ILoggerFactory loggerFactory,
            ISettingsService settingsService,
            AllocationManager allocationManager,
            ExchangeFactoryService exchangeFactoryService)
        {
            _logger = loggerFactory.CreateLogger<AlgorithmService>();
            _allocationManager = allocationManager;
            _exchangeFactoryService = exchangeFactoryService;
            _settingsService = settingsService;
            _algorithms = new Dictionary<Type, bool>();

            InitialiseAlgorithms();
            SetInitialAllocation();
        }

        /// <inheritdoc />
        public ResponseObject StartAlgorithm(Type algorithm)
        {
            // Check if algorithm exists
            if (!_algorithms.ContainsKey(algorithm))
            {
                throw new ArgumentException($"Algorithm {algorithm} was not found in AlgorithmService");
            }

            // Check if algorithm is in a stopped state
            if (_algorithms[algorithm])
            {
                return new ResponseObject(ResponseCode.Error, "Algorithm was already started.");
            }

            // TODO: Figure out which container to get

            // Build container
            var container = _exchangeFactoryService.BuildContainer(_allocationManager.GetWeakAllocationManager());

            // TODO: Initialise algorithm with container

            // Set status Running to True
            _algorithms[algorithm] = true;

            // Return a success
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public ResponseObject StopAlgorithm(Type algorithm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the initial allocation in AllocationManager
        /// </summary>
        private void SetInitialAllocation()
        {
            // TODO: Get allocation from Settings

            // Sets initial configuration
            _allocationManager.SetInitialConfiguration(null);
        }

        /// <summary>
        /// Initialises _algorithms with the implemented algorithms
        /// </summary>
        private void InitialiseAlgorithms()
        {
            // Make sure this method is working with a fresh dictionary
            if (_algorithms.Count != 0)
            {
                throw new InvalidConstraintException("_algorithms should not have any algorithms initialised " +
                                                     "or be reinitialised");
            }

            /* You have to add your algorithm manually and the method checks if all instances
             * of BaseAlgorithm were added. This prevents dangling algorithms until a better system
             * has been created.
             */

            // Add algorithms (intentionally hardcoded)
            _algorithms.Add(typeof(SimpleBandWagonAlgorithmSettings), false);

            // Get all implemented algorithms
            var algorithms =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(BaseAlgorithm<>))
                select type;

            // Checks if all algorithms were added to the dictionary
            foreach (Type algorithm in algorithms)
            {
                if (!_algorithms.ContainsKey(algorithm))
                {
                    throw new InvalidConstraintException($"The algorithm {algorithm} was not added to " +
                                                     $"AlgorithmService._algorithms. Is this on purpose?");
                }
            }
        }
    }
}
