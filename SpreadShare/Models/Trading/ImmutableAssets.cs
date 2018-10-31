using System.Collections.Generic;

namespace SpreadShare.Models.Trading
{
    class ImmutableAssets : Assets
    {
        public ImmutableAssets(List<ExchangeBalance> balances)
        {
            base(balances);
        }
    }
}
