using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    public interface IFetchCandles
    {
        ResponseObject Connect();
    }
}
