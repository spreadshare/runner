using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;

namespace SpreadShare.Strategy
{
    abstract class BaseStrategy : IStrategy
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITradingService _tradingService;
        public StateManager StateManager { get; private set; }

        /// <summary>
        /// BaseConstrcutor: Provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory"></param>
        protected BaseStrategy(ILoggerFactory loggerFactory, ITradingService tradingService)
        {
            _loggerFactory = loggerFactory;
            _tradingService = tradingService;
        }

        /// <summary>
        /// Start strategy with initial state using a StateManager
        /// </summary>
        public void Start()
        {
            StateManager = new StateManager(GetInitialState(), _loggerFactory, _tradingService);
        }

        public abstract State GetInitialState();
    }
}
