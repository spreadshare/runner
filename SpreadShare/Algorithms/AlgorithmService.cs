using System;
using System.Collections.Generic;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Utilities;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Manages running algorithms.
    /// </summary>
    internal class AlgorithmService : IAlgorithmService
    {
        private readonly ILogger _logger;
        private readonly ExchangeFactoryService _exchangeFactoryService;
        private readonly Dictionary<Type, IBaseAlgorithm> _algorithms;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmService"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provides logging capabilities.</param>
        /// <param name="allocationManager">Set allocations on startup.</param>
        /// <param name="exchangeFactoryService">Provides containers for algorithms.</param>
        public AlgorithmService(
            ILoggerFactory loggerFactory,
            AllocationManager allocationManager,
            ExchangeFactoryService exchangeFactoryService)
        {
            _logger = loggerFactory.CreateLogger<AlgorithmService>();
            _exchangeFactoryService = exchangeFactoryService;
            _algorithms = new Dictionary<Type, IBaseAlgorithm>();

            if (Program.CommandLineArgs.Trading)
            {
                // Sets initial configuration
                allocationManager.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>
                {
                    [Exchange.Binance] = Configuration.Instance.EnabledAlgorithm.GetAsDictionary(),
                });
            }
        }

        /// <inheritdoc />
        public ResponseObject StartAlgorithm<T>(AlgorithmConfiguration configuration)
            where T : IBaseAlgorithm
        {
            if (!Reflections.AlgorithmMatchesConfiguration(typeof(T), configuration.GetType()))
            {
                return new ResponseObject(ResponseCode.Error, $"Provided settings object is of type {configuration.GetType()} and does not match {typeof(T)}");
            }

            // Check if algorithm is in a stopped state
            if (_algorithms.ContainsKey(typeof(T)))
            {
                return new ResponseObject(ResponseCode.Error, $"Algorithm {typeof(T).Name} was already started.");
            }

            // Prevent starting real deployment without flag.
            if (Configuration.Instance.EnabledAlgorithm.Exchange != Exchange.Backtesting && !Program.CommandLineArgs.Trading)
            {
                throw new PermissionDeniedException($"Cannot deploy {typeof(T).Name} on non trading mode.");
            }

            // Figure out which container to get
            IBaseAlgorithm algorithm = (IBaseAlgorithm)Activator.CreateInstance(typeof(T));

            // Build container
            var container = _exchangeFactoryService.BuildContainer<T>(configuration);

            // Initialise algorithm with container
            var startResponse = algorithm.Start(configuration, container);

            if (!startResponse.Success)
            {
                return startResponse;
            }

            // Run backtest asynchronously by awaiting the timer.
            if (Configuration.Instance.EnabledAlgorithm.Exchange == Exchange.Backtesting)
            {
                if (container.TimerProvider is BacktestTimerProvider backtestTimer)
                {
                    while (!backtestTimer.Finished)
                    {
                        Thread.Sleep(10);
                    }

                    (bool hasErrored, var error) = backtestTimer.ErrorRegister;
                    if (hasErrored)
                    {
                        backtestTimer.LogOutput();
                        _logger.LogError(error, "Exception during backtesting");
                        return new ResponseObject(ResponseCode.Error, error.ToString());
                    }
                }
                else
                {
                    throw new InvalidStateException(
                        $"Error starting {typeof(T).Name}, configuration defined exchange as {Configuration.Instance.EnabledAlgorithm.Exchange}, " +
                        $"but the container did not contain a {typeof(ExchangeTimerProvider).Name}");
                }
            }
            else
            {
                // Set status Running to True
                _algorithms[typeof(T)] = algorithm;
            }

            // Return a success
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc/>
        public ResponseObject StartAlgorithm(Type algorithm, AlgorithmConfiguration configuration)
        {
           return GetType()
                .GetMethod(nameof(StartAlgorithm), new[] { typeof(AlgorithmConfiguration) })
                .MakeGenericMethod(algorithm)
                .Invoke(this, new object[] { configuration }) as ResponseObject;
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
    }
}
