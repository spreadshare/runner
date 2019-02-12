using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Backtesting;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Provider implementation for backtest purposes.
    /// </summary>
    internal class BacktestDataProvider : AbstractDataProvider
    {
        private static Dictionary<string, BacktestingCandle[]> _buffers;
        private readonly BacktestTimerProvider _timer;
        private readonly DatabaseContext _database;
        private readonly int _millisecondsCandleWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="database">The backtest database database.</param>
        /// <param name="timerProvider">Used to keep track of time.</param>
        /// <param name="backtestCommunicationService">Communicates with backtesting and provides order updates.</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestTimerProvider timerProvider, BacktestCommunicationService backtestCommunicationService)
            : base(loggerFactory, backtestCommunicationService)
        {
            _database = database;
            _timer = timerProvider;

            // Calculate once.
            _millisecondsCandleWidth = TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth).Milliseconds;
            if (_buffers == null)
            {
                _buffers = new Dictionary<string, BacktestingCandle[]>();
            }
        }

        /// <summary>
        /// Sets the DataProvider that implements this BackTestTradingProvider.
        /// </summary>
        public DataProvider ParentImplementation { private get; set; }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            var candle = FindCandle(pair, _timer.CurrentTime.ToUnixTimeMilliseconds());
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair) => GetCurrentPriceLastTrade(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => GetCurrentPriceLastTrade(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            long timestamp = _timer.CurrentTime.ToUnixTimeMilliseconds();
            var candleNow = FindCandle(pair, timestamp);
            var candleBack = FindCandle(pair, timestamp - (long)(hoursBack * 3600L * 1000L));

            return new ResponseObject<decimal>(ResponseCode.Success, candleNow.Average / candleBack.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width)
        {
            var time = _timer.CurrentTime;
            var result = new BacktestingCandle[limit];
            for (int i = 0; i < limit; i++)
            {
                result[i] = FindCandle(pair, time.ToUnixTimeMilliseconds());
                time -= TimeSpan.FromMinutes(5);
            }

            return new ResponseObject<BacktestingCandle[]>(result);
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            decimal max = -1;
            TradingPair maxTradingPair = null;

            foreach (var tradingPair in pairs)
            {
                var performanceQuery = GetPerformancePastHours(tradingPair, hoursBack);
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
        /// Get a value of the portfolio using the parent wrapper.
        /// </summary>
        /// <param name="portfolio">Portfolio to evaluate.</param>
        /// <returns>value of portfolio in base currency.</returns>
        public decimal ValuatePortfolioInBaseCurrency(Portfolio portfolio)
        {
            return ParentImplementation.ValuatePortfolioInBaseCurrency(portfolio);
        }

        /// <summary>
        /// Find candle that matches the timestamp most closely.
        /// </summary>
        /// <param name="pair">Candle's trading pair.</param>
        /// <param name="timestamp">CreatedTimestamp to match.</param>
        /// <returns>Candle matching timestamp most closely.</returns>
        private BacktestingCandle FindCandle(TradingPair pair, long timestamp)
        {
            if (!_buffers.ContainsKey(pair.ToString()))
            {
                Logger.LogCritical($"Building a new buffer for {pair}");
                _buffers.Add(
                    pair.ToString(),
                    _database.Candles.AsNoTracking()
                        .Where(x => x.TradingPair == pair.ToString())
                        .OrderBy(x => x.Timestamp)
                        .ToArray());
                Logger.LogCritical($"Done building the buffer for {pair}");
            }

            // Minus one to prevent reading candles whose close is in the future.
            long index = ((timestamp - _buffers[pair.ToString()][0].Timestamp) / _millisecondsCandleWidth) - 1;
            if (index < 0)
            {
                Logger.LogError("Got request for a candle that exists before the scope of available data," +
                                "Did you use GetTopPerformance without allowing enough time offset?");
                throw new InvalidOperationException("Tried to read outside backtest data buffer");
            }

            return _buffers[pair.ToString()][index];
        }
    }
}