using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
    interface IStrategy
    {
        ResponseObject Start();
    }
}
