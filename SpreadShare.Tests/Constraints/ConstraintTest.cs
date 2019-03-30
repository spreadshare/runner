using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace SpreadShare.Tests.Constraints
{
    public abstract class ConstraintTest : BaseTest
    {
        protected ConstraintTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Deserializer = new DeserializerBuilder().Build();
        }

        protected IDeserializer Deserializer { get; }
    }
}