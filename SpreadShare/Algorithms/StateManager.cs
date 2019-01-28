using System;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms
{
    /// <summary>
    /// Object managing the active state and related resources.
    /// </summary>
    /// <typeparam name="T">The type of the parent algorithm settings.</typeparam>
    internal class StateManager<T> : IDisposable
        where T : AlgorithmSettings
    {
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly DatabaseContext _database;
        private readonly IDisposable _timerObserver;
        private readonly IDisposable _tradingObserver;

        private State<T> _activeState;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateManager{T}"/> class.
        /// Sets active state with an initial state and sets basic settings.
        /// </summary>
        /// <param name="algorithmSettings">The settings of the algorithm settings.</param>
        /// <param name="initial">Initial state of the algorithm.</param>
        /// <param name="container">Exchange service container.</param>
        /// <param name="database">The database context for logging state switches.</param>
        public StateManager(
            T algorithmSettings,
            EntryState<T> initial,
            ExchangeProvidersContainer container,
            DatabaseContext database)
        {
            Guard.Argument(initial).NotNull();
            lock (_lock)
            {
                // Setup logging
                _logger = container.LoggerFactory.CreateLogger(GetType());
                _loggerFactory = container.LoggerFactory;

                // Link the parent algorithm setting
                AlgorithmSettings = algorithmSettings;

                Container = container;

                // Setup observing
                var periodicObserver = new ConfigurableObserver<long>(
                    time =>
                    {
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

                // Bind the database
                _database = database;

                // Setup initial state
                _activeState = initial;
                _activeState.Activate(algorithmSettings, container, _loggerFactory);
            }
        }

        /// <summary>
        /// Gets the container with exchange service providers.
        /// </summary>
        private ExchangeProvidersContainer Container { get; }

        /// <summary>
        /// Gets a link to the algorithm settings.
        /// </summary>
        private T AlgorithmSettings { get; }

        /// <summary>
        /// Gets the current active state.
        /// </summary>
        private string CurrentState
        {
            get
            {
                lock (_lock)
                {
                    return _activeState.GetType().ToString().Split('+').Last();
                }
            }
        }

        /// <summary>
        /// Evaluates the active state's market condition predicate.
        /// </summary>
        public void OnMarketConditionEval()
        {
            lock (_lock)
            {
                var next = _activeState.OnMarketCondition(Container.DataProvider);
                SwitchState(next);
            }
        }

        /// <summary>
        /// Evaluates the active state's order update condition.
        /// </summary>
        /// <param name="order">Update of a certain order.</param>
        public void OnOrderUpdateEval(OrderUpdate order)
        {
            lock (_lock)
            {
                var next = _activeState.OnOrderUpdate(order);
                SwitchState(next);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the StateManager.
        /// </summary>
        /// <param name="disposing">Actually do it.</param>
        protected virtual void Dispose(bool disposing)
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
                    $"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()} at {Container.TimerProvider.CurrentTime}");

                // Full cycle -> increase TradeID
                if (child is EntryState<T>)
                {
                    Container.TradingProvider.IncrementTradeId();
                }

                // Add state switch event to the database
                _database.StateSwitchEvents.Add(new StateSwitchEvent(
                    Container.TimerProvider.CurrentTime.ToUnixTimeMilliseconds(),
                    CurrentState,
                    child.GetType().Name));

                _activeState = child;

                _activeState.Activate(AlgorithmSettings, Container, _loggerFactory);
            }
        }

        /// <summary>
        /// Evaluate the timer of the current state.
        /// </summary>
        private void EvaluateStateTimer()
        {
            lock (_lock)
            {
                if (!_activeState.TimerTriggered && _activeState.EndTime <= Container.TimerProvider.CurrentTime)
                {
                    _activeState.TimerTriggered = true;
                    var next = _activeState.OnTimerElapsed();
                    SwitchState(next);
                }
            }
        }
    }
}
