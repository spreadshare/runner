using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.ZeroMQ
{
    interface IZeroMqService
    {
        /*
         * Interface documentation is found in the PROTOCOL.md
         */

        ResponseObject Start();
    }
}
