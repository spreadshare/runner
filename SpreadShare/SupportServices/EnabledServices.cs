namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Settings for enable status of services
    /// </summary>
    internal class EnabledServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnabledServices"/> class.
        /// Create Settings object for service enabling
        /// </summary>
        /// <param name="strategyService">Whether strategy services should be enabled</param>
        /// <param name="tradingService">Whether trading services should be enabled</param>
        /// <param name="userService">Whether user services should be enabled</param>
        /// <param name="zeroMqService">Whether ZeroMQ services should be enabled</param>
        public EnabledServices(bool strategyService, bool tradingService, bool userService, bool zeroMqService)
        {
            StrategyService = strategyService;
            TradingService = tradingService;
            UserService = userService;
            ZeroMqService = zeroMqService;
        }

        /// <summary>
        /// Gets a value indicating whether strategy service is enabled
        /// </summary>
        public bool StrategyService { get; }

        /// <summary>
        /// Gets a value indicating whether trading service is enabled
        /// </summary>
        public bool TradingService { get; }

        /// <summary>
        /// Gets a value indicating whether user service is enabled
        /// </summary>
        public bool UserService { get; }

        /// <summary>
        /// Gets a value indicating whether ZeroMQ service is enabled
        /// </summary>
        public bool ZeroMqService { get; }
    }
}