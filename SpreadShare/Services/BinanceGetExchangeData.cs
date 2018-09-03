using System;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;
using SpreadShare.Services.Support;
using SpreadShare.Strategy;

namespace SpreadShare.Services
{
    class BinanceGetExchangeData : IGetExchangeData
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        private readonly BaseStrategy _strategy;

        public BinanceGetExchangeData(DatabaseContext dbContext, ILoggerFactory loggerFactory, 
            IStrategy strategy)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<BinanceGetExchangeData>();
            _strategy = (BaseStrategy)strategy;
        }

        public async Task Connect()
        {
            using (var client = new BinanceSocketClient())
            {
                await GetCandles(client, "bnbbtc");
                await GetCandles(client, "ethbtc");
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
