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
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public BinanceTradingService TradingService;
        public BinanceUserService UserService;

        
        public string CurrentState => _activeState.GetType().ToString().Split('+').Last();

        /// <summary>
        /// Constructor: Initialise the active state with an initial state
        /// </summary>
        /// <param name="initial">First state to be active. Can't be null</param>
        /// <param name="loggerFactory">Provides logger for StateManager and states</param>
        public StateManager(State initial, ILoggerFactory loggerFactory, ITradingService tradingService, IUserService userService)
        {
            lock (_lock)
            {
                _logger = loggerFactory.CreateLogger("StateManager");
                _loggerFactory = loggerFactory;
                TradingService = tradingService as BinanceTradingService;
                UserService = userService as BinanceUserService;
                UserService.OrderUpdateHandler += OnOrderUpdate;
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
            if (child == null) throw new Exception("Given child state is null. State manager may only contain non-null states");

            _logger.LogInformation($"STATE SWITCH: {CurrentState} ---> {child.GetType().ToString().Split('+').Last()}");

            Context c = _activeState.Context;
            Interlocked.Exchange(ref _activeState, child);
            child.Activate(c, this, _loggerFactory);
        }


        /// <summary>
        /// Example of an action
        /// </summary>
        public void OnCandle(Candle c)
        {
            lock (_lock)
            {
                _activeState.OnCandle(c);
            }
        }

        private void OnOrderUpdate(object sender, BinanceStreamOrderUpdate order) {
            lock (_lock)
            {
                _activeState.OnOrderUpdate(order);
            }
        }
    }
}
