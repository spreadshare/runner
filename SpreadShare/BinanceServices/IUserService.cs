using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal interface IUserService
    {
        ResponseObject Start();
        ResponseObject<Assets> GetPortfolio();
    }
}
