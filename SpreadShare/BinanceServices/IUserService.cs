using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal interface IUserService
    {
        void Start();
        Assets GetPortfolio();
    }
}
