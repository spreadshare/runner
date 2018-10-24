using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;

namespace SpreadShare.ExchangeServices.Backtesting
{
    /// <summary>
    /// Trading provider implementation for backtesting purposes.
    /// </summary>
    internal class BacktestTradingProvider : AbstractTradingProvider
    {
        private readonly BacktestOutputAgent _agent;
        private readonly BacktestTimerProvider _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestTradingProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="agent">Output agent for writing backtest report</param>
        /// <param name="timer">timer provider for registering trades</param>
        public BacktestTradingProvider(ILoggerFactory loggerFactory, BacktestOutputAgent agent, BacktestTimerProvider timer)
            : base(loggerFactory)
        {
            _agent = agent;
            _timer = timer;
        }

        /// <inheritdoc />
        public override ResponseObject PlaceFullMarketOrder(CurrencyPair pair, OrderSide side, decimal amount)
        {
            _agent.RegisterTradeEvent(_timer.CurrentTime, pair.Right, pair.Left, side, amount, new Currency("BNB"), 69);
            return new ResponseObject(ResponseCode.Success);
        }

        /// <inheritdoc />
        public override ResponseObject CancelOrder(CurrencyPair pair, long orderId)
        {
            throw new System.NotImplementedException();
        }
    }
}