using System;
using System.Linq.Expressions;
using SpreadShare.Utilities;
using Xunit;

namespace SpreadShare.Tests.UtilitiesTests
{
    public class SwitchTypeTests
    {
        [Fact]
        public void SwitchTypeHappyFlow()
        {
            bool executed = false;
            var item = new TypeA();
            item.Switch(
                SwitchType.Case<TypeA>(() => executed = true),
                SwitchType.Default(() => throw new Exception("This case should not be executed.")));
            Assert.True(executed);
        }

        [Fact]
        public void SwitchTypeNoValidCase()
        {
            bool executed = false;
            var item = new TypeB();
            item.Switch(
                SwitchType.Case<TypeA>(() => executed = true));
            Assert.False(executed);
        }

        [Fact]
        public void SwitchTypeNoCases()
        {
            var item = new TypeA();
            item.Switch();
        }

        [Fact]
        public void SwitchTypeCasePassesToNext()
        {
            bool executed = false;
            var item = new TypeB();
            item.Switch(
                SwitchType.Case<TypeA>(() => throw new Exception("This case should not be executed.")),
                SwitchType.Case<TypeB>(() => executed = true));
            Assert.True(executed);
        }

        [Fact]
        public void SwitchTypeDerivedCase()
        {
            bool executed = false;
            var item = new TypeAStar();
            item.Switch(
                SwitchType.Case<TypeB>(() => throw new Exception("This case should not be executed.")),
                SwitchType.Case<TypeA>(() => executed = true),
                SwitchType.Default(() => throw new Exception("This case should not be executed.")));
            Assert.True(executed);
        }

        [Fact]
        public void SwitchDefaultOnly()
        {
            bool executed = false;
            var item = new TypeA();
            item.Switch(
                SwitchType.Default(() => executed = true));
            Assert.True(executed);
        }

        [Fact]
        public void SwitchDefaultExecuted()
        {
            bool executed = false;
            var item = new TypeB();
            item.Switch(
                SwitchType.Case<TypeA>(() => Expression.Empty()),
                SwitchType.Default(() => executed = true));
            Assert.True(executed);
        }

        private class TypeA
        {
        }

        private class TypeB
        {
        }

        private class TypeAStar : TypeA
        {
        }
    }
}