using System;
using System.Collections.Generic;
using System.Linq;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ExchangeCommunicationService.Binance;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

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
        /// <param name="loggerFactory">Used to create output stream</param>
        /// <param name="communications">For communication with Binance</param>
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

            decimal ret = response.Data.Asks.Max(x => x.Price);
            return new ResponseObject<decimal>(ResponseCode.Success, ret);
        }

        /// <summary>
        /// Gets past performance in the past hours
        /// </summary>
        /// <param name="pair">Trading pair to obtain performance of</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>A response object with the performance on success</returns>
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

        /// <summary>
        /// Gets the top performing trading pair
        /// </summary>
        /// <param name="pairs">A list of trading pairs to evaluate</param>
        /// <param name="hoursBack">Amount of hours to look back</param>
        /// <param name="endTime">DateTime marking the end of the period</param>
        /// <returns>Top performing trading pair</returns>
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
    }
}
