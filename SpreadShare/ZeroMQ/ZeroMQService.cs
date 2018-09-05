using System.Threading.Tasks;

namespace SpreadShare.ZeroMQ
{
    class ZeroMqService : IZeroMqService
    {
        public Task BroadcastMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public Task StartCommandReceiver()
        {
            throw new System.NotImplementedException();
        }
    }
}
