using System.Data;
using System.IO;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.SupportServices.Configuration.ConstraintAttributes;
using Xunit;
using Xunit.Abstractions;

// Allow 'unused' classes and 'unused' constructor parameters
#pragma warning disable CA1812, CA1801

namespace SpreadShare.Tests.Constraints
{
    public class CanBeConstructedTests : ConstraintTest
    {
        public CanBeConstructedTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void CanBeConstructedHappyFlow()
        {
            const string input = "Item: a_string ";
            var obj = Deserializer.Deserialize<ConstructableHappyFlowObject>(new StringReader(input));
            ConfigurationValidator.ValidateConstraintsRecursively(obj);
        }

        [Fact]
        public void CanBeConstructedPrivateConstructor()
        {
            const string input = "Item: a_string";
            var obj = Deserializer.Deserialize<ConstructablePrivateConstructorObject>(new StringReader(input));
            Assert.Throws<InvalidConstraintException>(() => ConfigurationValidator.ValidateConstraintsRecursively(obj));
        }

        private class ConstructableHappyFlowObject
        {
            [CanBeConstructed(typeof(ConstructableHappyFlow))]
            public string Item { get; private set; }

            private class ConstructableHappyFlow
            {
                public ConstructableHappyFlow(string s)
                {
                }
            }
        }

        private class ConstructablePrivateConstructorObject
        {
            [CanBeConstructed(typeof(ConstructablePrivateConstructor))]
            public string Item { get; private set; }

            private class ConstructablePrivateConstructor
            {
                private ConstructablePrivateConstructor(string s)
                {
                }
            }
        }
    }
}

#pragma warning restore CA1812, CA1801
