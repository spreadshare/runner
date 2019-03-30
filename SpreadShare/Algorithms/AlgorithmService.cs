using System;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Utilities;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Manages running algorithms.
    /// </summary>
    internal class AlgorithmService
    {
        private readonly ILogger _logger;
        private readonly ExchangeFactoryService _exchangeFactoryService;

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

            if (Program.CommandLineArgs.Trading)
            {
                // Sets initial configuration
                try
                {
                    allocationManager.SetInitialConfiguration(Configuration.Instance.EnabledAlgorithm.Allocation);
                }
                catch (AllocationUnavailableException)
                {
                    Program.ExitProgramWithCode(ExitCode.AllocationUnavailable);
                }
            }
        }

        /// <summary>
        /// Starts the algorithm using a custom configuration object.
        /// </summary>
        /// <param name="algorithm">Algorithm to start.</param>
        /// <param name="configuration">Configuration object.</param>
        /// <returns>If the algorithm was started successfully.</returns>
        public ResponseObject StartAlgorithm(Type algorithm, AlgorithmConfiguration configuration)
        {
            if (!Reflections.AlgorithmMatchesConfiguration(algorithm, configuration.GetType()))
            {
                return new ResponseObject(ResponseCode.Error, $"Provided settings object is of type {configuration.GetType()} and does not match {algorithm}");
            }

            // Prevent starting real deployment without flag.
            if (Configuration.Instance.EnabledAlgorithm.Exchange != Exchange.Backtesting && !Program.CommandLineArgs.Trading)
            {
                throw new PermissionDeniedException($"Cannot deploy {algorithm.Name} on non trading mode.");
            }

            // Call StartAlgorithm<T> to start the algorithm
            return GetType()
                .GetMethod(
                    nameof(StartAlgorithm),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    new[] { typeof(AlgorithmConfiguration) },
                    null)
                .MakeGenericMethod(algorithm)
                .Invoke(this, new object[] { configuration }) as ResponseObject;
        }

        /// <summary>
        /// Starts the algorithm using a custom configuration object.
        /// </summary>
        /// <param name="configuration">Configuration object.</param>
        /// <typeparam name="T">The type of algorithm to start.</typeparam>
        /// <returns>If the algorithm was started successfully.</returns>
        private ResponseObject StartAlgorithm<T>(AlgorithmConfiguration configuration)
            where T : IBaseAlgorithm
        {
            // Figure out which container to get
            var algorithm = (IBaseAlgorithm)Activator.CreateInstance(typeof(T));

            // Build container
            var container = _exchangeFactoryService.BuildContainer<T>(configuration);

            // Initialise algorithm with container
            var startResponse = algorithm.Start(configuration, container);

            if (!startResponse.Success)
            {
                return startResponse;
            }

            // Run backtest asynchronously by awaiting the timer.
            return Configuration.Instance.EnabledAlgorithm.Exchange == Exchange.Backtesting
                ? WaitTillBacktestFinished(container.TimerProvider as BacktestTimerProvider)
                : new ResponseObject(ResponseCode.Success);
        }

        /// <summary>
        /// Run timer provider until finished.
        /// </summary>
        /// <param name="backtestTimer">Active timer of the backtest.</param>
        /// <returns>Whether the backtest is finished.</returns>
        private ResponseObject WaitTillBacktestFinished(BacktestTimerProvider backtestTimer)
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

            return new ResponseObject(ResponseCode.Success);
        }
    }
}
