using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;

namespace SpreadShare.ExchangeServices.ProvidersBinance
{
    /// <summary>
    /// Provides data gathering capabilities for Binance.
    /// </summary>
    internal class BinanceDataProvider : AbstractDataProvider
    {
        private readonly BinanceCommunicationsService _communications;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceDataProvider"/> class.
        /// </summary>
        /// <param name="loggerFactory">Used to create output stream.</param>
        /// <param name="communications">For communication with Binance.</param>
        public BinanceDataProvider(ILoggerFactory loggerFactory, BinanceCommunicationsService communications)
            : base(loggerFactory, communications)
        {
            _communications = communications;
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceLastTrade(TradingPair pair)
        {
            var client = _communications.Client;
            var response = client.GetPrice(pair.ToString());
            if (!response.Success)
            {
                Logger.LogWarning($"Could not fetch price for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            return new ResponseObject<decimal>(ResponseCode.Success, response.Data.Price);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceTopBid(TradingPair pair)
        {
            var client = _communications.Client;
            var response = client.GetOrderBook(pair.ToString());
            if (!response.Success)
            {
                Logger.LogWarning($"Could not fetch top bid for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            decimal ret = response.Data.Bids.Max(x => x.Price);
            return new ResponseObject<decimal>(ResponseCode.Success, ret);
        }

        /// <inheritdoc/>
        public override ResponseObject<decimal> GetCurrentPriceTopAsk(TradingPair pair)
        {
            var client = _communications.Client;
            var response = client.GetOrderBook(pair.ToString());
            if (!response.Success)
            {
                Logger.LogWarning($"Could not fetch top ask for {pair} from binance");
                return new ResponseObject<decimal>(ResponseCode.Error);
            }

            decimal ret = response.Data.Asks.Min(x => x.Price);
            return new ResponseObject<decimal>(ResponseCode.Success, ret);
        }

        /// <inheritdoc />
        public override ResponseObject<decimal> GetPerformancePastHours(TradingPair pair, double hoursBack)
        {
            if (hoursBack <= 0)
            {
                throw new ArgumentException("Argument hoursBack should be larger than 0.");
            }

            var client = _communications.Client;

            var startTime = DateTimeOffset.Now - TimeSpan.FromHours(hoursBack);
            var endTime = DateTimeOffset.Now;
            var response = client.GetKlines(pair.ToString(), KlineInterval.OneMinute, startTime.UtcDateTime, endTime.UtcDateTime);

            if (response.Success)
            {
                var length = response.Data.Length;
                var first = response.Data[0].Open;
                var last = response.Data[length - 1].Close;
                return new ResponseObject<decimal>(ResponseCode.Success, last / first);
            }

            Logger.LogCritical(response.Error.Message);
            Logger.LogWarning($"Could not fetch price for {pair} from binance!");
            return new ResponseObject<decimal>(ResponseCode.Error);
        }

        /// <inheritdoc />
        public override ResponseObject<BacktestingCandle[]> GetCandles(TradingPair pair, int limit, CandleWidth width)
        {
            var client = _communications.Client;
            var result = new BacktestingCandle[limit];
            int chunkSize = Configuration.Instance.BinanceClientSettings.CandleRequestSize;
            var tasks = new (int, Task<CallResult<BinanceKline[]>>)[(limit / chunkSize) + 1];

            // Start the offset all the way back.
            TimeSpan offset = TimeSpan.FromMinutes(limit * (int)width);

            // Create tasks for retrieving the candles in reverse.
            for (int i = tasks.Length - 1; i >= 0; i--)
            {
                // Last task only needs to gather the remaining candles.
                int amount = i == 0
                    ? limit % chunkSize
                    : chunkSize;

                // Decrement the offset with the current job.
                offset -= TimeSpan.FromMinutes(amount * (int)width);

                tasks[i] =
                    (amount, client.GetKlinesAsync(
                                symbol: pair.ToString(),
                                interval: BinanceUtilities.ToExternal(width),
                                startTime: DateTime.UtcNow - offset - TimeSpan.FromMinutes((amount + 1) * (int)width), // Request one extra candle, sometimes binance comes one short.
                                endTime: DateTime.UtcNow - offset,
                                limit: amount + 10));
            }

            // Insert all the task results into the resulting array.
            // i = task.id
            // q = result_candle.id
            // p = task_candle.id
            for (int i = 0, q = 0; i < tasks.Length; i++)
            {
                var (size, response) = (tasks[i].Item1, tasks[i].Item2.Result);
                if (!response.Success)
                {
                    return new ResponseObject<BacktestingCandle[]>(ResponseCode.Error, response.Error.Message);
                }

                // If needed, strip the extra requested candle, then reverse the array to match present -> past.
                var candles = response.Data.Skip(response.Data.Length - size).Reverse().ToArray();
                for (int p = 0; p < size; p++, q++)
                {
                    // Parse to the right data structure, and insert in the resulting array.
                    var x = candles[p];
                    result[q] = new BacktestingCandle(
                        new DateTimeOffset(x.OpenTime).ToUnixTimeMilliseconds(),
                        x.Open,
                        x.Close,
                        x.High,
                        x.Low,
                        x.Volume,
                        pair.ToString());
                }
            }

            return new ResponseObject<BacktestingCandle[]>(ResponseCode.Success, result.ToArray());
        }

        /// <inheritdoc />
        public override ResponseObject<Tuple<TradingPair, decimal>> GetTopPerformance(List<TradingPair> pairs, double hoursBack)
        {
            Guard.Argument(hoursBack).NotNegative(x => $"{nameof(hoursBack)} cannot be negative: {x}");

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
    }
}
