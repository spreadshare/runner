using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    interface IUserService
    {
        void Start();
        Assets GetPortfolio();
    }
}
