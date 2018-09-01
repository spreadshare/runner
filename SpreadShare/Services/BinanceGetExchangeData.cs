using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.DependencyInjection;
using SpreadShare.Models;

namespace SpreadShare.Services
{
    class BinanceGetExchangeData : IGetExchangeData
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;

        public BinanceGetExchangeData(DatabaseContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<BinanceGetExchangeData>();
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
                    _logger.LogInformation(c.ToString());
                });
                candles.Data.Closed += () => _logger.LogInformation("Socket closed");
                candles.Data.Error += e => _logger.LogInformation("Socket error: {1}", e);

            }
        }
    }
}
