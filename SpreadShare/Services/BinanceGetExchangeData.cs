using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
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

        public BinanceGetExchangeData(DatabaseContext dbContext, ILoggerFactory loggerFactory, IStrategy strategy)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<BinanceGetExchangeData>();
            _strategy = (BaseStrategy)strategy;
        }

        public async Task Connect()
        {
            _logger.LogInformation("Got here");
            using (var client = new BinanceSocketClient())
            {
                var candles = await client.SubscribeToKlineStreamAsync("bnbbtc", KlineInterval.OneMinute, data =>
                {
                    Candle c = new Candle(data.Data);
                    _dbContext.Add(c);
                    _strategy.StateManager.OnSomeAction();
                    _logger.LogInformation(c.ToString());
                });
                candles.Data.Closed += () => _logger.LogInformation("Socket closed");
                candles.Data.Error += e => _logger.LogInformation("Socket error: {1}", e);

            }
        }
    }
}
