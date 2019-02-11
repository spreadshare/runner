using System.Collections.Generic;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.Tests.Stubs
{
    internal class TestPortfolioFetcher : IPortfolioFetcherService
    {
        public ResponseObject<Portfolio> GetPortfolio(Exchange exchange)
        {
            // These values are used verbatim in the tests, be careful when changing them.
            return new ResponseObject<Portfolio>(new Portfolio(
                new Dictionary<Currency, Balance>
                {
                    { new Currency("ETH"), new Balance(new Currency("ETH"), 100M, 0) },
                    { new Currency("EOS"), new Balance(new Currency("EOS"), 1200M, 0) },
                    { new Currency("BNB"), new Balance(new Currency("BNB"), 337.69M, 0) },
                }));
        }
    }
}