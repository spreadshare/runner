using System.IO;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Xunit;
using Xunit.Abstractions;
using YamlDotNet.Serialization;

namespace SpreadShare.Tests.Constraints
{
    public class CompatibleCandleWidthTests : BaseTest
    {
        private readonly IDeserializer _deserializer;

        public CompatibleCandleWidthTests(ITestOutputHelper logger)
            : base(logger)
        {
            _deserializer = new DeserializerBuilder().Build();
        }

        [Fact]
        public void CompatibleCandleWidthHappyFlow()
        {
            const string input = "Value: TenMinutes";
            var obj = _deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            ConfigurationValidator.ValidateConstraintsRecursively(obj);
        }

        [Fact]
        public void CompatibleCandleWidthTooSmall()
        {
            const string input = "Value: OneMinute";
            var obj = _deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            Assert.Throws<InvalidConfigurationException>(
                () => ConfigurationValidator.ValidateConstraintsRecursively(obj));
        }

        [Fact]
        public void CompatibleCandleWidthSame()
        {
            const string input = "Value: FiveMinutes";
            var obj = _deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            ConfigurationValidator.ValidateConstraintsRecursively(obj);
        }

        [Fact]
        public void CompatibleCandleWidthNotDivisible()
        {
            Assert.Equal(7, (int)CandleWidth.DONOTUSETestEntry);
            const string input = "Value: DONOTUSETestEntry";
            var obj = _deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            Assert.Throws<InvalidConfigurationException>(
                () => ConfigurationValidator.ValidateConstraintsRecursively(obj));
        }

        // Class is instantiated via Activator
        #pragma warning disable CA1812
        private class CompatibleCandleWidthObject
        {
            [CompatibleCandleWidth]
            public string Value { get; set; }
        }
        #pragma warning restore CA1812
    }
}