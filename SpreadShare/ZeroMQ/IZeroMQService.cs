using SpreadShare.Models;

namespace SpreadShare.ZeroMQ
{
    internal interface IZeroMqService
    {
        /*
         * Interface documentation is found in the PROTOCOL.md
         */

        ResponseObject Start();
    }
}
