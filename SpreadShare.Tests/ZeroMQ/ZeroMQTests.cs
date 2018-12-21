using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;
using NetMQ.Sockets;
using SpreadShare.ZeroMQ;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.ZeroMQ
{
    /// <summary>
    /// Tests of the <ZeroMqService cref="ZeroMqService"/> class.
    /// </summary>
    public class ZeroMqTests : BaseTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroMqTests"/> class.
        /// </summary>
        /// <param name="outputHelper">Output helper that writes to TestOutput.</param>
        public ZeroMqTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            var serviceProvider = ServiceProviderSingleton.Instance.ServiceProvider;
            var zeroMqService = serviceProvider.GetService<IZeroMqService>();
            zeroMqService.Start();
        }

        /// <summary>
        /// Tests the broadcasting functionality of the ZeroMQ services.
        /// </summary>
        /// <param name="topics">Topics to subscribe to.</param>
        [Theory(Skip = "Test takes too long")]
        [InlineData("topic_status", "topic_holdtime")]
        public void BroadcastingAllTopicsTest(params string[] topics)
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

                    if (failcount <= 4)
                    {
                        continue;
                    }

                    var notReceived = string.Empty;
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
