using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal interface IUserService
    {
        Task Start();
        Assets GetPortfolio();
    }
}
