using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models.Trading;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ExchangeServices
{
    public class DataProviderTests : BaseProviderTests
    {
        private readonly DataProvider _data;

        public DataProviderTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var container = ExchangeFactoryService.BuildContainer(typeof(TemplateAlgorithm));
            _data = container.DataProvider;
        }

        [Fact]
        public void AverageTrueRangeHappyFlow()
        {
            Assert.True(_data.GetAverageTrueRange(TradingPair.Parse("VETETH"), 5).Success);
        }
    }
}