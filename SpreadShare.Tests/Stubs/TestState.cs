using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.Providers;

namespace SpreadShare.Tests.Stubs
{
    internal class TestState : State<TemplateAlgorithmSettings>
    {
        protected override void Run(TradingProvider trading, DataProvider data)
        {
        }
    }
}