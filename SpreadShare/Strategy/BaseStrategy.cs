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
        /// Used to get information from the appsettings.json
        /// </summary>
        protected readonly SettingsService SettingsService;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ITradingService _tradingService;
        private readonly IUserService _userService;

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
            _loggerFactory = loggerFactory;
            _tradingService = tradingService;
            _userService = userService;
            SettingsService = settingsService as SettingsService;
        }

        /// <summary>
        /// Gets the strategy's settings.
        /// </summary>
        protected abstract T Settings { get; }

        /// <summary>
        /// Start strategy with the initial state using a StateManager
        /// </summary>
        /// <returns>Whether the stategy started succesfully</returns>
        public virtual ResponseObject Start()
        {
            var stateManager = new StateManager<T>(
                Settings,
                GetInitialState(),
                _loggerFactory,
                _tradingService,
                _userService);

            return new ResponseObject(ResponseCodes.Success);
        }

        /// <summary>
        /// Gets the initial state of the strategy
        /// </summary>
        /// <returns>The initial state of the strategy</returns>
        protected abstract State<T> GetInitialState();
    }
}
