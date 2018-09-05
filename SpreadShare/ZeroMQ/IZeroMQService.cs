using System.Threading.Tasks;

namespace SpreadShare.ZeroMQ
{
    interface IZeroMqService
    {
        /*
         * Interface documentation is found in the PROTOCOL.md
         */

        Task BroadcastMessage(string message);
        Task StartCommandReceiver();
    }
}
