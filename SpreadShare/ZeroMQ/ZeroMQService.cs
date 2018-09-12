using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace SpreadShare.ZeroMQ
{
    class ZeroMqService : IZeroMqService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public ZeroMqService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ZeroMqService>();
        }

        public Task BroadcastMessage(string message)
        {
            throw new System.NotImplementedException();
        }

        public Task StartCommandReceiver()
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://*:5555");

                while (true)
                {
                    string message;
                    try
                    {
                        message = server.ReceiveFrameString();
                    }
                    catch (NetMQ.FiniteStateMachineException e)
                    {
                        _logger.LogError("NetMQ.FiniteStateMachineException occured");
                        continue;
                    }

                    _logger.LogInformation($"Received {message}");

                    // Parse command
                    Command c;
                    try
                    {
                        c = Command.GetCommand(message, _loggerFactory);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical($"Exception occured: {e.Message}");
                        server.SendFrame(new Response(Response.Type.failure, e.Message).ToJson());
                        continue;
                    }

                    // Run action
                    try
                    {
                        c.Action();
                    }
                    catch (Exception e)
                    {
                        server.SendFrame(new Response(Response.Type.error, $"Action errored: {e.Message}").ToJson());
                        continue;
                    }

                    server.SendFrame(new Response(Response.Type.success, "Action completed successfully").ToJson());
                }
            }
        }
    }
}
