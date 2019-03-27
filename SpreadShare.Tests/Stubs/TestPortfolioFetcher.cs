using System.Collections.Generic;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.Tests.Stubs
{
    // Class is instantiated via the Activator
    #pragma warning disable CA1812

    internal class TestPortfolioFetcher : IPortfolioFetcherService
    {
        public static Portfolio GetStaticPortfolio()
        {
            // These values are used verbatim in the tests, be careful when changing them.
            return new Portfolio(
                new Dictionary<Currency, Balance>
                {
                    { new Currency("ETH"), new Balance(new Currency("ETH"), 100M, 0) },
                    { new Currency("EOS"), new Balance(new Currency("EOS"), 120000M, 0) },
                    { new Currency("BNB"), new Balance(new Currency("BNB"), 337.69M, 0) },
                    { new Currency("BTC"), new Balance(new Currency("BTC"), 5M, 0) },
                });
        }

        public ResponseObject<Portfolio> GetPortfolio() => new ResponseObject<Portfolio>(GetStaticPortfolio());
    }

    #pragma warning disable CA1812
}