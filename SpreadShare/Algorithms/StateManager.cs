using System;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Object managing the active state and related resources.
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm configuration.</typeparam>
    internal sealed class StateManager<T> : Observable<Type>, IDisposable
        where T : AlgorithmConfiguration
    {
        private readonly object _lock = new object();
        private readonly T _configuration;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDisposable _timerObserver;
        private readonly IDisposable _tradingObserver;

        private State<T> _activeState;
        private bool _active;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateManager{T}"/> class.
        /// Sets active state with an initial state and sets basic configuration.
        /// </summary>
        /// <param name="algorithmConfiguration">The configuration of the algorithm.</param>
        /// <param name="container">Exchange service container.</param>
        /// <param name="initial">The initial state.</param>
        public StateManager(
            T algorithmConfiguration,
            ExchangeProvidersContainer container,
            EntryState<T> initial)
        {
            lock (_lock)
            {
                // Setup logging
                _logger = container.LoggerFactory.CreateLogger(GetType());
                _loggerFactory = container.LoggerFactory;

                _configuration = algorithmConfiguration;

                // Link the parent algorithm configuration
                AlgorithmConfiguration = algorithmConfiguration;

                Container = container;

                // Setup observing
                var periodicObserver = new ConfigurableObserver<long>(
                    time =>
                    {
                        if (!_active)
                        {
                            Activate(initial);
                        }

                        OnMarketConditionEval();
                        EvaluateStateTimer();
                    },
                    () => { },
                    e => { });
                _timerObserver = container.TimerProvider.Subscribe(periodicObserver);

                var orderObserver = new ConfigurableObserver<OrderUpdate>(
                    OnOrderUpdateEval,
                    () => { },
                    e => { });
                _tradingObserver = container.TradingProvider.Subscribe(orderObserver);
            }
        }

        /// <summary>
        /// Gets the container with exchange service providers.
        /// </summary>
        private ExchangeProvidersContainer Container { get; }

        /// <summary>
        /// Gets a link to the algorithm configuration.
        /// </summary>
        private T AlgorithmConfiguration { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Activates the state manager by feeding it the initial state.
        /// </summary>
        /// <param name="initial">The initial state.</param>
        private void Activate(EntryState<T> initial)
        {
            Guard.Argument(initial).NotNull();
            if (_active)
            {
                throw new InvalidOperationException("Cannot activate the state manager when it is already active.");
            }

            _active = true;

            UpdateObservers(initial.GetType());

            // Setup initial state
            _activeState = initial;
            SwitchState(_activeState.Activate(AlgorithmConfiguration, Container, _loggerFactory));
        }

        /// <summary>
        /// Evaluates the active state's market condition predicate.
        /// </summary>
        private void OnMarketConditionEval()
        {
            lock (_lock)
            {
                try
                {
                    var next = _activeState.OnMarketCondition(Container.DataProvider);
                    SwitchState(next);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Evaluates the active state's order update condition.
        /// </summary>
        /// <param name="order">Update of a certain order.</param>
        private void OnOrderUpdateEval(OrderUpdate order)
        {
            lock (_lock)
            {
                try
                {
                    var next = _activeState.OnOrderUpdate(order);
                    SwitchState(next);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// Dispose the StateManager.
        /// </summary>
        /// <param name="disposing">Actually do it.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _loggerFactory.Dispose();
                    _timerObserver.Dispose();
                    _tradingObserver.Dispose();
                    Container.TradingProvider.Dispose();
                }
            }
        }

        /// <summary>
        /// Switches the active state to the given state, only to be used by states.
        /// </summary>
        /// <param name="child">State to switch to.</param>
        /// <exception cref="Exception">Child can't be null.</exception>
        private void SwitchState(State<T> child)
        {
            // This function is safe because it is executed in the locked context of the OnX callback functions
            Guard.Argument(child).NotNull();
            if (child is NothingState<T>)
            {
                return;
            }

            lock (_lock)
            {
                _logger.LogInformation(
                    $"STATE SWITCH: {_active.GetType().Name} ---> {child.GetType().Name} at {Container.TimerProvider.CurrentTime}");

                // Full cycle -> increase TradeID
                if (child is EntryState<T>)
                {
                    Container.TradingProvider.IncrementTradeId();
                }

                // Add state switch event to the database
                UpdateObservers(child.GetType());

                _activeState = child;

                // Keep switching if the run method yields a new state.
                var next = _activeState.Activate(AlgorithmConfiguration, Container, _loggerFactory);
                if (!(next is NothingState<T>))
                {
                    if (Program.CommandLineArgs.Trading)
                    {
                        _logger.LogDebug($"Sleeping {(int)_configuration.CandleWidth} to prevent rapid trading.");
                        Thread.Sleep((int)TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth)
                            .TotalMilliseconds);
                    }

                    SwitchState(next);
                }
            }
        }

        /// <summary>
        /// Evaluate the timer of the current state.
        /// </summary>
        private void EvaluateStateTimer()
        {
            lock (_lock)
            {
                try
                {
                    if (!_activeState.TimerTriggered && _activeState.EndTime <= Container.TimerProvider.CurrentTime)
                    {
                        _activeState.TimerTriggered = true;
                        var next = _activeState.OnTimerElapsed();
                        SwitchState(next);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    throw;
                }
            }
        }
    }
}
