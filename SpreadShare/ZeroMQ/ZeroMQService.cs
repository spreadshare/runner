using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZeroMQ;

namespace SpreadShare.ZeroMQ
{
    class ZeroMqService : IZeroMqService
    {
        private readonly ILogger _logger;

        public ZeroMqService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ZeroMqService>();
        }

        public Task BroadcastMessage(string message)
        {
            var y = Command.GetCommand("{\"command\": \"commandX\"}");
            throw new System.NotImplementedException();
        }

        public Task StartCommandReceiver()
        {
            // Create
            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:5555");

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                        Console.WriteLine("Received {0}", request.ReadString());

                        // Do some work
                        Thread.Sleep(1);

                        // Send
                        //responder.Send(new ZFrame(name));
                    }
                }
            }
            throw new System.NotImplementedException();
        }
    }
}
