using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.Strategy.Implementations;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.SettingsService;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Base class for all strategies
    /// </summary>
    internal abstract class BaseStrategy<T> : IStrategy
       where T : BaseStrategy<T>
    {
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly ITradingService _tradingService;
        protected readonly IUserService _userService;
        protected readonly ISettingsService _settingsService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStrategy"/> class.
        /// Implements and provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory">Provided logger creating capabilities</param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user data fetching capabilities</param>
        /// <param name="settingsService">Provides acces to global settings</param>
        protected BaseStrategy(
            ILoggerFactory loggerFactory,
            ITradingService tradingService,
            IUserService userService,
            ISettingsService settingsService)
        {
            _loggerFactory = loggerFactory;
            _tradingService = tradingService;
            _userService = userService;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Gets the StateManager
        /// </summary>
        public StateManager<T> StateManager { get; set; }

        /// <summary>
        /// Start strategy with the initial state using a StateManager
        /// </summary>
        /// <returns>Whether the stategy started succesfully</returns>
        public abstract ResponseObject Start();
        
        
        /// <summary>
        /// Gets the initial state of the strategy
        /// </summary>
        /// <returns>The initial state of the strategy</returns>
        public abstract State<T> GetInitialState();
    }
}
