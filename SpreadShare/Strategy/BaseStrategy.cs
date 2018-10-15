using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Base class for all strategies
    /// </summary>
    /// <typeparam name="T">The specific strategy that is associated with it</typeparam>
    internal abstract class BaseStrategy<T> : IStrategy
       where T : StrategySettings
    {
        /// <summary>
        /// Used for creating logging output
        /// </summary>
        protected readonly ILoggerFactory LoggerFactory;

        /// <summary>
        /// Used for trading
        /// </summary>
        protected readonly ITradingService TradingService;

        /// <summary>
        /// Used to get information about the specific account
        /// </summary>
        protected readonly IUserService UserService;

        /// <summary>
        /// Used to get information from the appsettings.json
        /// </summary>
        protected readonly SettingsService SettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStrategy{T}"/> class.
        /// Implements and provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory">Provided logger creating capabilities</param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user data fetching capabilities</param>
        /// <param name="settingsService">Provides access to global settings</param>
        protected BaseStrategy(
            ILoggerFactory loggerFactory,
            ITradingService tradingService,
            IUserService userService,
            ISettingsService settingsService)
        {
            LoggerFactory = loggerFactory;
            TradingService = tradingService;
            UserService = userService;
            SettingsService = settingsService as SettingsService;
        }

        /// <summary>
        /// Gets the strategy's settings.
        /// </summary>
        protected abstract T Settings { get; }

        /// <summary>
        /// Gets or sets the StateManager
        /// </summary>
        protected StateManager<T> StateManager { get; set; }

        /// <summary>
        /// Start strategy with the initial state using a StateManager
        /// </summary>
        /// <returns>Whether the stategy started succesfully</returns>
        public virtual ResponseObject Start()
        {
            StateManager = new StateManager<T>(
                Settings,
                GetInitialState(),
                LoggerFactory,
                TradingService,
                UserService);

            return new ResponseObject(ResponseCodes.Success);
        }

        /// <summary>
        /// Gets the initial state of the strategy
        /// </summary>
        /// <returns>The initial state of the strategy</returns>
        public abstract State<T> GetInitialState();
    }
}
