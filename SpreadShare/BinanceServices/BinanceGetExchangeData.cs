using System;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Strategy;
using SpreadShare.SupportServices;

namespace SpreadShare.BinanceServices
{
    class BinanceFetchCandles : IFetchCandles
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        private readonly BaseStrategy _strategy;
        private readonly IConfiguration _configuration;

        public BinanceFetchCandles(DatabaseContext dbContext, ILoggerFactory loggerFactory, 
            IStrategy strategy, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<BinanceFetchCandles>();
            _strategy = (BaseStrategy)strategy;
            _configuration = configuration;
        }

        public async Task Connect()
        {
            using (var client = new BinanceSocketClient())
            {
                foreach (var tradingPair in _configuration.GetSection("BinanceClientSettings:tradingPairs").GetChildren().AsEnumerable())
                {
                    await GetCandles(client, tradingPair.Value);
                }
            }
        }

        private async Task<CallResult<BinanceStreamSubscription>> GetCandles(BinanceSocketClient client, string tradingPair)
        {
            // Temporary Candle
            Candle prev = new Candle { OpenTime = new DateTime() };

            // Subscription
            var candles = await client.SubscribeToKlineStreamAsync(tradingPair, KlineInterval.OneMinute, data =>
            {
                Candle c = new Candle(data.Data);
                _logger.LogDebug("Received Candle \t{0}", c.ToString());

                // New Candle
                if (!c.OpenTime.Equals(prev.OpenTime))
                {
                    _logger.LogInformation("Received new Candle\n\t{0}", prev.ToString());

                    if (prev.Symbol != null)
                    {
                        _dbContext.Add(prev);
                        _strategy.StateManager.OnCandle(prev);
                    }
                    prev = c;
                }

            });
            candles.Data.Closed += () => _logger.LogInformation("Socket closed");
            candles.Data.Error += e => _logger.LogInformation("Socket error: {1}", e);
            return candles;
        }
    }
}
