using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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
        private readonly BacktestTimerProvider _timer;
        private readonly BacktestBuffers _buffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output.</param>
        /// <param name="database">The backtest database database.</param>
        /// <param name="timerProvider">Used to keep track of time.</param>
        public BacktestDataProvider(ILoggerFactory loggerFactory, DatabaseContext database, BacktestTimerProvider timerProvider)
            : base(loggerFactory, timerProvider)
        {
            _timer = timerProvider;
            _buffers = new BacktestBuffers(database, Logger);
        }

        /// <summary>
        /// Sets the DataProvider that implements this BackTestTradingProvider.
        /// </summary>
        public DataProvider ParentImplementation { private get; set; }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            var candle = FindCandle(pair, _timer.CurrentTime.ToUnixTimeMilliseconds(), Configuration.Instance.CandleWidth);
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Average);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair)
        {
            // Ask for the open of one candle in the future
            var timestamp = (_timer.CurrentTime + TimeSpan.FromMinutes((int)Configuration.Instance.CandleWidth)).ToUnixTimeMilliseconds();
            var candle = FindCandle(pair, timestamp, Configuration.Instance.CandleWidth);
            return new ResponseObject<decimal>(ResponseCode.Success, candle.Open);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair) => GetCurrentPriceTopBid(pair);

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            long timestamp = _timer.CurrentTime.ToUnixTimeMilliseconds();
            var candleNow = FindCandle(pair, timestamp, Configuration.Instance.CandleWidth);
            var candleBack = FindCandle(pair, timestamp - (long)(hoursBack * 3600L * 1000L), Configuration.Instance.CandleWidth);

            return new ResponseObject<decimal>(ResponseCode.Success, candleNow.Average / candleBack.Average);
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

        /// <inheritdoc />
        public override ResponseObject<BacktestingCandle[]> GetCustomCandles(TradingPair pair, int numberOfCandles, int width)
        {
            var result = new BacktestingCandle[numberOfCandles];
            var time = _timer.CurrentTime;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = FindCandle(pair, time.ToUnixTimeMilliseconds(), width);
                time -= TimeSpan.FromMinutes((int)width);
            }

            return new ResponseObject<BacktestingCandle[]>(result);
        }

        /// <inheritdoc />
        protected override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit)
        {
            var time = _timer.CurrentTime;
            var result = new BacktestingCandle[limit];
            var width = Configuration.Instance.CandleWidth;
            for (var i = 0; i < limit; i++)
            {
                result[i] = FindCandle(pair, time.ToUnixTimeMilliseconds(), width);
                time -= TimeSpan.FromMinutes(width);
            }

            return new ResponseObject<BacktestingCandle[]>(result);
        }

        /// <summary>
        /// Find candle that matches the timestamp most closely.
        /// </summary>
        /// <param name="pair">Candle's trading pair.</param>
        /// <param name="timestamp">CreatedTimestamp to match.</param>
        /// <param name="channelWidth">The width of the candles.</param>
        /// <returns>Candle matching timestamp most closely and the index at which it was encountered.</returns>
        private BacktestingCandle FindCandle(TradingPair pair, long timestamp, int channelWidth)
        {
            var buffer = _buffers.GetCandles(pair, channelWidth);
            var millisecondsCandleWidth = (int)TimeSpan.FromMinutes(channelWidth).TotalMilliseconds;

            // Minus one to prevent reading candles whose close is in the future.
            long index = ((timestamp - buffer[0].Timestamp) / millisecondsCandleWidth) - 1;
            if (index < 0)
            {
                Logger.LogError("Got request for a candle that exists before the scope of available data");
                throw new InvalidOperationException("Tried to read outside backtest data buffer");
            }

            return buffer[index];
        }
    }
}