using SpreadShare.Models;

namespace SpreadShare.ZeroMQ
{
    public interface IZeroMqService
    {
        /*
         * Interface documentation is found in the PROTOCOL.md
         */

        ResponseObject Start();
    }
}
