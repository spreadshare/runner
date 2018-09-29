using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    internal abstract class BaseStrategy : IStrategy
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ITradingService _tradingService;
        public StateManager StateManager { get; private set; }

        /// <summary>
        /// BaseConstrcutor: Provides dependencies required by the StateManager
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="tradingService">Provides trading capabilities</param>
        protected BaseStrategy(ILoggerFactory loggerFactory,
            ITradingService tradingService)
        {
            _loggerFactory = loggerFactory;
            _tradingService = tradingService;
        }

        /// <summary>
        /// Start strategy with initial state using a StateManager
        /// </summary>
        public ResponseObject Start()
        {
            StateManager = new StateManager(
                GetInitialState(), 
                _loggerFactory, 
                _tradingService
            );
            return new ResponseObject(ResponseCodes.Success);
        }

        public abstract State GetInitialState();
    }
}
