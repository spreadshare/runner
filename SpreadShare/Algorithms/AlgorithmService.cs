using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms.Implementations;
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
        public ResponseObject StartAlgorithm(Type algorithmType)
        {
            // Check if algorithm exists
            if (!_algorithms.ContainsKey(algorithmType))
            {
                throw new ArgumentException($"Algorithm {algorithmType} was not found in AlgorithmService");
            }

            // Check if algorithm is in a stopped state
            if (_algorithms[algorithmType])
            {
                return new ResponseObject(ResponseCode.Error, "Algorithm was already started.");
            }

            // Figure out which container to get
            BaseAlgorithm algorithm = (BaseAlgorithm)Activator.CreateInstance(algorithmType);
            AlgorithmSettings settings = GetSettings(algorithm.GetSettingsType);
            Exchange exchangeEnum = settings.Exchange;

            // Build container
            var container = _exchangeFactoryService.BuildContainer(
                exchangeEnum,
                algorithmType,
                _allocationManager.GetWeakAllocationManager());

            // Initialise algorithm with container
            algorithm.Start(settings, container);

            // Set status Running to True
            _algorithms[algorithmType] = true;

            // Return a success
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public ResponseObject StopAlgorithm(Type algorithmType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the initial allocation in AllocationManager
        /// </summary>
        private void SetInitialAllocation()
        {
            // Sets initial configuration
            _allocationManager.SetInitialConfiguration((_settingsService as SettingsService).AllocationSettings);
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
            _algorithms.Add(typeof(SimpleBandWagonAlgorithm), false);

            // Get all implemented algorithms
            var algorithms =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(BaseAlgorithm))
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

        /// <summary>
        /// Gets the exchange from the algorithm settings
        /// </summary>
        /// <param name="algorithmSettingsType">Type of the settings of the algorithm</param>
        /// <returns>The settings of the algorithm with configured values</returns>
        private AlgorithmSettings GetSettings(Type algorithmSettingsType)
        {
            // Get type of SettingsService as declared in Startup.cs
            Type settingsType = _settingsService.GetType();

            // Get public non-static properties in settings
            var publicInstanceProperties = settingsType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            // Get all properties with returnType = algorithmSettingsType
            List<PropertyInfo> list = publicInstanceProperties
                .Where(methodInfo => methodInfo.PropertyType == algorithmSettingsType).ToList();

            // Check if any property with searched settings return type is declared
            if (list.Count < 1)
            {
                throw new InvalidConstraintException($"No methods with type {algorithmSettingsType} " +
                                                     $"were declared in ${settingsType}");
            }

            // Check if multiple properties with searched settings return type are declared
            if (list.Count > 1)
            {
                throw new InvalidConstraintException($"Multiple methods with type {algorithmSettingsType} " +
                                                     $"were declared in ${settingsType}");
            }

            return (AlgorithmSettings)list[0].GetValue(_settingsService);
        }
    }
}
