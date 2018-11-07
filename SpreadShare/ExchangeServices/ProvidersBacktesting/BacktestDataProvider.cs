using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Provider implementation for backtest purposes.
    /// </summary>
    internal class BacktestDataProvider : AbstractDataProvider
    {
        private const int HalfCandleInterval = 30000;
        private readonly BacktestTimerProvider _timer;
        private readonly DatabaseContext _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output</param>
        /// <param name="database">The backtest database database</param>
        /// <param name="timerProvider">Used to keep track of time</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestTimerProvider timerProvider)
            : base(loggerFactory)
        {
            _database = database;
            _timer = timerProvider;
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            var candle = FindCandle(pair, _timer.CurrentMinuteEpoc);
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => GetCurrentPriceLastTrade(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => GetCurrentPriceLastTrade(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack, DateTimeOffset endTime)
        {
            long timestamp = endTime.ToUnixTimeMilliseconds();
            var candleNow = FindCandle(pair, timestamp);
            var candleBack = FindCandle(pair, timestamp - (long)(hoursBack * 3600 * 1000));

            return new ResponseObject<decimal>(ResponseCode.Success, candleNow.Average / candleBack.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack, DateTime endTime)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            decimal max = -1;
            TradingPair maxTradingPair = null;

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
                    return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Error, performanceQuery.ToString());
                }

                if (max < performance)
                {
                    max = performance;
                    maxTradingPair = tradingPair;
                }
            }

            if (maxTradingPair == null)
            {
                return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Error, "No trading pairs defined");
            }

            return new ResponseObject<Tuple<TradingPair, decimal>>(ResponseCode.Success, new Tuple<TradingPair, decimal>(maxTradingPair, max));
        }

        /// <summary>
        /// Find candle that matches the timestamp most closely.
        /// </summary>
        /// <param name="pair">Candle's trading pair</param>
        /// <param name="timestamp">Timestamp to match</param>
        /// <returns>Candle matching timestamp most closely</returns>
        private BacktestingCandle FindCandle(TradingPair pair, long timestamp)
        {
            /* Throws exception if no or multiple candles are returned. This is expected behaviour as the backtesting data
             * should have fixed timestamps.
             */
            return _database.Candles
                .AsNoTracking()
                .Single(c =>
                       timestamp - HalfCandleInterval <= c.Timestamp
                    && c.Timestamp <= timestamp + HalfCandleInterval
                    && c.TradingPair == pair.ToString());
        }
    }
}