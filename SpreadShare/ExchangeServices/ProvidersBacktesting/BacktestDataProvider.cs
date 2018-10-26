using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Provider;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Provider implementation for backtest purposes.
    /// </summary>
    internal class BacktestDataProvider : AbstractDataProvider
    {
        private readonly BacktestTimerProvider _timer;
        private readonly DatabaseContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="context">The backtest database context</param>
        /// <param name="timerProvider">Used to keep track of time</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, DatabaseContext context, BacktestTimerProvider timerProvider)
            : base(loggerFactory)
        {
            _context = context;
            _timer = timerProvider;
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(CurrencyPair pair)
        {
            var candle = _context.Candles.First(x => x.Timestamp == _timer.CurrentMinuteEpoc);
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(CurrencyPair pair)
        {
            return GetCurrentPriceLastTrade(pair);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(CurrencyPair pair)
        {
            return GetCurrentPriceLastTrade(pair);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(CurrencyPair pair, double hoursBack, DateTimeOffset endTime)
        {
            var now = endTime.ToUnixTimeMilliseconds();
            var rawBack = now - (long)(hoursBack * 3600 * 1000);
            var roundedBack = rawBack - (rawBack % 60000);
            var candleNow = _context.Candles.First(x => x.Timestamp == now && x.TradingPair == pair.ToString());
            var candleBack = _context.Candles.First(x => x.Timestamp == roundedBack && x.TradingPair == pair.ToString());
            return new ResponseObject<decimal>(ResponseCode.Success, candleNow.Average / candleBack.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<CurrencyPair, decimal>> GetTopPerformance(List<CurrencyPair> pairs, double hoursBack, DateTime endTime)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            decimal max = -1;
            CurrencyPair maxTradingPair = null;

            foreach (var tradingPair in pairs)
            {
                var performanceQuery = GetPerformancePastHours(tradingPair, hoursBack, endTime);
                decimal performance;
                if (performanceQuery.Code == ResponseCode.Success)
                {
                    performance = performanceQuery.Data;
                }
                else
                {
                    Logger.LogWarning($"Error fetching performance data: {performanceQuery}");
                    return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Error, performanceQuery.ToString());
                }

                if (max < performance)
                {
                    max = performance;
                    maxTradingPair = tradingPair;
                }
            }

            if (maxTradingPair == null)
            {
                return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Error, "No trading pairs defined");
            }

            return new ResponseObject<Tuple<CurrencyPair, decimal>>(ResponseCode.Success, new Tuple<CurrencyPair, decimal>(maxTradingPair, max));
        }
    }
}