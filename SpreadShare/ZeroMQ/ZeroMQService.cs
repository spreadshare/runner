using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SpreadShare.Models;

namespace SpreadShare.ZeroMQ
{
    internal class ZeroMqService : IZeroMqService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor: Provides loggers
        /// </summary>
        /// <param name="loggerFactory"></param>
        public ZeroMqService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ZeroMqService>();
        }

        /// <summary>
        /// Start both threads
        /// </summary>
        /// <returns></returns>
        public ResponseObject Start()
        {
            Thread broadcastService = new Thread(StartBroadcastService);
            broadcastService.Start();

            Thread commandReceiver = new Thread(StartCommandReceiver);
            commandReceiver.Start();

            return new ResponseObject(ResponseCodes.Success);
        }

        /// <summary>
        /// Start pub-sub publisher for broadcasting status and holdtime
        /// </summary>
        private void StartBroadcastService()
        {
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Options.SendHighWatermark = 1000;
                pubSocket.Bind("tcp://*:5556");

                while (true)
                {
                    pubSocket.SendMoreFrame("topic_status").SendFrame("TODO");
                    pubSocket.SendMoreFrame("topic_holdtime").SendFrame("TODO");
                    Thread.Sleep(60000);
                }
            }
        }

        /// <summary>
        /// Start req-rep listener for commands
        /// </summary>
        private void StartCommandReceiver()
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
                    catch (FiniteStateMachineException)
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
