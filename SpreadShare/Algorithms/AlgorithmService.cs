using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;
using SpreadShare.Utilities;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Manages running algorithms.
    /// </summary>
    internal class AlgorithmService : IAlgorithmService
    {
        private readonly ILogger _logger;
        private readonly SettingsService _settingsService;
        private readonly AllocationManager _allocationManager;
        private readonly ExchangeFactoryService _exchangeFactoryService;
        private readonly Dictionary<Type, IBaseAlgorithm> _algorithms;
        private readonly DatabaseContext _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging capabilities</param>
        /// <param name="settingsService">Provides allocation and algorithm settings</param>
        /// <param name="database">The database context</param>
        /// <param name="allocationManager">Provides allocation management</param>
        /// <param name="exchangeFactoryService">Provides containers for algorithms</param>
        public AlgorithmService(
            ILoggerFactory loggerFactory,
            SettingsService settingsService,
            DatabaseContext database,
            AllocationManager allocationManager,
            ExchangeFactoryService exchangeFactoryService)
        {
            _logger = loggerFactory.CreateLogger<AlgorithmService>();
            _database = database;
            _allocationManager = allocationManager;
            _exchangeFactoryService = exchangeFactoryService;
            _settingsService = settingsService;
            _algorithms = new Dictionary<Type, IBaseAlgorithm>();

            SetInitialAllocation();
        }

        /// <inheritdoc />
        public ResponseObject StartAlgorithm(Type algorithmType)
        {
            // Check if type is an algorithm
            if (!Reflections.IsAlgorithm(algorithmType))
            {
                return new ResponseObject(ResponseCode.Error, $"Provided type {algorithmType} is not an algorithm.");
            }

            // Check if algorithm is in a stopped state
            if (_algorithms.ContainsKey(algorithmType))
            {
                return new ResponseObject(ResponseCode.Error, "Algorithm was already started.");
            }

            // Figure out which container to get
            IBaseAlgorithm algorithm = (IBaseAlgorithm)Activator.CreateInstance(algorithmType);

            AlgorithmSettings settings = _settingsService.GetAlgorithSettings(algorithmType);

            // Build container
            var container = _exchangeFactoryService.BuildContainer(algorithmType);

            // Start the timer provider
            container.TimerProvider.RunPeriodicTimer();

            // Initialise algorithm with container
            var startResponse = algorithm.Start(settings, container, _database);

            if (!startResponse.Success)
            {
                return startResponse;
            }

            // Set status Running to True
            _algorithms[algorithmType] = algorithm;

            // Return a success
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public ResponseObject StopAlgorithm(Type algorithmType)
        {
            Guard.Argument(algorithmType).Require(
                Reflections.IsAlgorithm,
                x => $"Cannot stop {x} for it is not an algorithm");

            if (!_algorithms.ContainsKey(algorithmType))
            {
                return new ResponseObject(ResponseCode.Success, $"{algorithmType} was already stopped");
            }

            _algorithms[algorithmType].Stop();
            _algorithms.Remove(algorithmType);

            return new ResponseObject(ResponseCode.Success);
        }

        /// <summary>
        /// Sets the initial allocation in AllocationManager
        /// </summary>
        private void SetInitialAllocation()
        {
            // Sets initial configuration
            _allocationManager.SetInitialConfiguration(_settingsService.AllocationSettings);
        }

        /// <summary>
        /// Gets the exchange from the algorithm settings
        /// </summary>
        /// <param name="algorithmSettingsType">Type of the settings of the algorithm</param>
        /// <returns>The settings of the algorithm with configured values</returns>
        private ResponseObject<AlgorithmSettings> GetSettings(Type algorithmSettingsType)
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
                var msg = $"No settings could be found for {algorithmSettingsType}. " +
                          "Did you add settings to appsettings.json?";
                return new ResponseObject<AlgorithmSettings>(ResponseCode.Error, msg);
            }

            // Check if multiple properties with searched settings return type are declared
            if (list.Count > 1)
            {
                var msg = $"Multiple settings were found for {algorithmSettingsType}. Do you have duplicate settings " +
                          "in appsettings.json?";
                return new ResponseObject<AlgorithmSettings>(ResponseCode.Error, msg);
            }

            return new ResponseObject<AlgorithmSettings>(
                ResponseCode.Success, (AlgorithmSettings)list[0].GetValue(_settingsService));
        }
    }
}
