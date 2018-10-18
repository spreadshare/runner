using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms.Common
{
    /// <summary>
    /// Object managing the active state and related resources
    /// </summary>
    /// <typeparam name="T">The type of the parent strategy settings</typeparam>
    internal class StateManager<T> : IDisposable
        where T : StrategySettings
    {
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private State<T> _activeState;
        private Timer _activeTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateManager{T}"/> class.
        /// Sets active state with an initial state and sets basic settings
        /// </summary>
        /// <param name="strategySettings">The settings of the strategy settings</param>
        /// <param name="initial">Initial state of the strategy</param>
        /// <param name="loggerFactory">LoggerFactory for creating loggers</param>
        /// <param name="tradingService">Instance of the trading service</param>
        /// <param name="userService">Instance of the user service</param>
        public StateManager(
            T strategySettings,
            State<T> initial,
            ILoggerFactory loggerFactory,
            ITradingService tradingService,
            IUserService userService)
        {
            // Setup logging
            _logger = loggerFactory.CreateLogger("StateManager");
            _loggerFactory = loggerFactory;

            // Setup trading services (gain access to abstract members)
            TradingService = tradingService as AbstractTradingService;
            UserService = userService as AbstractUserService;

            // Link the parent strategy setting
            StrategySettings = strategySettings;

            // Setup initial state
            _activeState = initial ?? throw new Exception("Given initial state is null. State manager may only contain non-null states");
            initial.Activate(this, _loggerFactory);
        }

        /// <summary>
        /// Gets an instance of the trading service
        /// </summary>
        public AbstractTradingService TradingService { get; }

        /// <summary>
        /// Gets an instance of the user service
        /// </summary>
        public AbstractUserService UserService { get; }

        /// <summary>
        /// Gets a link to the strategy settings.
        /// </summary>
        public T StrategySettings { get; }

        /// <summary>
        /// Gets the current active state
        /// </summary>
        private string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Switches the active state to the given state, only to be used by states
        /// </summary>
        /// <param name="child">State to switch to</param>
        /// <exception cref="Exception">Child can't be null</exception>
        public void SwitchState(State<T> child)
        {
            // This function is safe because it is executed in the locked context of the OnX callback functions
            if (child == null)
            {
                throw new Exception("Given child state is null. State manager may only contain non-null states");
            }

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            Interlocked.Exchange(ref _activeState, child);
            child.Activate(this, _loggerFactory);
        }

        /// <summary>
        /// Creates new Timer object that waits and then executes callback
        /// </summary>
        /// <param name="minutes">Time to wait</param>
        public void SetTimer(uint minutes)
        {
            // Ensure the previous timer has gone out.
            _activeTimer?.Stop();
            _activeTimer = new Timer(minutes, _loggerFactory, () =>
            {
                // Callback returned after waiting period
                lock (_lock)
                {
                    /* State.OnTimer should return Success, while states without implementing a timer
                     * will return NotDefined by default.
                    */
                    var response = _activeState.OnTimer();
                    if (!response.Success)
                    {
                        _logger.LogInformation($"Timer callback was not used by state. Response Code: {response}");
                    }
                }
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object's resource
        /// </summary>
        /// <param name="disposing">Whether to dispose the resources of the object</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _activeTimer.Dispose();
                _loggerFactory.Dispose();
            }
        }
    }
}
