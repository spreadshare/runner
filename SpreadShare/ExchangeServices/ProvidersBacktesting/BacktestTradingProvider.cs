using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider
    {
        private readonly BacktestOutputLogger _backtestOutputLogger;
        private readonly BacktestTimerProvider _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="timer">timer provider for registering trades</param>
        public BacktestTradingProvider(ILoggerFactory loggerFactory, BacktestTimerProvider timer)
            : base(loggerFactory)
        {
            _timer = timer;

            // Output backtestOutputLogger for writing backtest report
            _backtestOutputLogger = new BacktestOutputLogger();
        }

        /// <inheritdoc />
        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side, decimal amount)
        {
            _backtestOutputLogger.RegisterTradeEvent(_timer.CurrentTime, pair.Right, pair.Left, side, amount, new Currency("BNB"), 69);
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            throw new System.NotImplementedException();
        }
    }
}