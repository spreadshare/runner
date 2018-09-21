using System;
using System.Linq;
using System.Threading;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    class StateManager
    {
        private State _activeState;
        private Timer _activeTimer;
        private readonly object _lock = new object();

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public AbstractTradingService TradingService;
        public AbstractUserService UserService;


        public string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Constructor: Initialise the active state with an initial state
        /// </summary>
        /// <param name="initial">First state to be active. Can't be null</param>
        /// <param name="loggerFactory">Provides logger for StateManager and states</param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user watching capabilities</param>
        public StateManager(State initial, ILoggerFactory loggerFactory, 
            ITradingService tradingService, IUserService userService)
        {
            lock (_lock)
            {
                // Setup logging
                _logger = loggerFactory.CreateLogger("StateManager");
                _loggerFactory = loggerFactory;

                // Setup Binance services
                TradingService = tradingService as AbstractTradingService;
                UserService = userService as AbstractUserService;
                try
                {
                    UserService.OrderUpdateHandler += OnOrderUpdate;
                }
                catch (NullReferenceException e)
                {
                    _logger.LogError("UserService could not be cast as AbstractUserService");
                    throw;
                }

                // Setup initial state
                _activeState = initial ?? throw new Exception("Given initial state is null. State manager may only contain non-null states");
                initial.Activate(new Context(), this, _loggerFactory);
            }
        }

        /// <summary>
        /// Switches the active state to the given state
        /// </summary>
        /// <param name="child">State to switch to</param>
        public void SwitchState(State child)
        {
            //This function is safe because it is executed in the locked context of the OnX callback functions
            if (child == null) throw new Exception("Given child state is null. State manager may only contain non-null states");

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            var c = _activeState.Context;
            Interlocked.Exchange(ref _activeState, child);
            child.Activate(c, this, _loggerFactory);
        }

        /// <summary>
        /// Creates new Timer object that waits and then executes callback
        /// </summary>
        /// <param name="ms">Time to wait</param>
        public void SetTimer(long ms)
        {
            _activeTimer = new Timer(ms, () =>
            {
                // Callback returned after waiting period
                lock (_lock)
                {
                    /* State.OnTimer should return Success, while states without implementing a timer
                     * will return NotDefined by default.
                    */
                    var response = _activeState.OnTimer();
                    _logger.LogInformation(response == State.ResponseCodes.Success
                        ? "Timer succesfully triggered!"
                        : $"Timer callback was not used by state. Response Code: {response}");
                }
            } );
        }



        /// <summary>
        /// Incoming candles trigger State.OnCandle
        /// </summary>
        /// <param name="c">Incoming candle</param>
        public void OnCandle(Candle c)
        {
            lock (_lock)
            {
                var response = _activeState.OnCandle(c);
                if (response != State.ResponseCodes.Success) {
                    _logger.LogInformation($"Candle was not processed by state. Response Code: { response }");
                }
            }
        }

        /// <summary>
        /// Callback method for EventHandler of UserService
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="order"></param>
        private void OnOrderUpdate(object sender, BinanceStreamOrderUpdate order)
        {
            lock (_lock)
            {
                var response = _activeState.OnOrderUpdate(order);
                if (response != State.ResponseCodes.Success) {
                    _logger.LogInformation($"Order update was not processed by the current state. Response Code: { response }");
                }
            }
        }
    }
}
