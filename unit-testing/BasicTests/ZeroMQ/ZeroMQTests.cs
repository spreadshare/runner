using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;
using NetMQ.Sockets;
using SpreadShare.ZeroMQ;
using Xunit;
using Xunit.Abstractions;

namespace Tests.ZeroMQ
{
    public class ZeroMqTests : BaseTest
    {
        public ZeroMqTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var zeroMqService = serviceProvider.GetService<IZeroMqService>();
            zeroMqService.Start();
        }

        [Theory]
        [InlineData("topic_status", "topic_holdtime")]
        public void BroadcastingAllTopicsTest(params string [] topics)
        {
            var received = new bool[topics.Length];
            var failcount = 0;

            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:5556");

                foreach (var topic in topics)
                {
                    subSocket.Subscribe(topic);
                }

                while (true)
                {
                    var messageTopicReceived = subSocket.ReceiveFrameString();
                    var messageReceived = subSocket.ReceiveFrameString();
                    received[topics.IndexOf(messageTopicReceived)] = true;
                    failcount++;

                    if (received.All(a => a))
                    {
                        Assert.True(true, "Messages from all topics have been received.");
                        return;
                    }
                    if (failcount <= 4) continue;


                    var notReceived = "";
                    for (var i = 0; i < received.Length; i++)
                    {
                        if (!received[i])
                        {
                            notReceived += topics[i] + ", ";
                        }
                    }
                    Assert.True(false, $"Not all messages from all topics have been received. Missing topics: {notReceived}");
                }
            }
        }
    }
}
