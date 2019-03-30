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
    public class CompatibleCandleWidthTests : ConstraintTest
    {
        public CompatibleCandleWidthTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void CompatibleCandleWidthHappyFlow()
        {
            const string input = "Value: 10";
            var obj = Deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            ConfigurationValidator.ValidateConstraintsRecursively(obj);
        }

        [Fact]
        public void CompatibleCandleWidthTooSmall()
        {
            const string input = "Value: 1";
            var obj = Deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            Assert.Throws<InvalidConfigurationException>(
                () => ConfigurationValidator.ValidateConstraintsRecursively(obj));
        }

        [Fact]
        public void CompatibleCandleWidthSame()
        {
            const string input = "Value: 5";
            var obj = Deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            ConfigurationValidator.ValidateConstraintsRecursively(obj);
        }

        [Fact]
        public void CompatibleCandleWidthNotDivisible()
        {
            const string input = "Value: 7";
            var obj = Deserializer.Deserialize<CompatibleCandleWidthObject>(new StringReader(input));
            Assert.Throws<InvalidConfigurationException>(
                () => ConfigurationValidator.ValidateConstraintsRecursively(obj));
        }

        // Class is instantiated via Activator
        #pragma warning disable CA1812
        private class CompatibleCandleWidthObject
        {
            [CompatibleCandleWidth]
            public int Value { get; set; }
        }
        #pragma warning restore CA1812
    }
}