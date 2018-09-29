using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    public interface IUserService
    {
        ResponseObject Start();
        ResponseObject<Assets> GetPortfolio();
    }
}
