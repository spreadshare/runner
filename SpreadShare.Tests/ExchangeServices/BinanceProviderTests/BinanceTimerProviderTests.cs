using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Exceptions.OrderExceptions;
using SpreadShare.SupportServices.ErrorServices;
using SpreadShare.Tests.Stubs;
using SpreadShare.Tests.Stubs.Binance;
using Xunit;

namespace SpreadShare.Tests.ExchangeServices.BinanceProviderTests
{
    public class BinanceTimerProviderTests : EnvironmentTest
    {
        private Action<BinanceTimerProvider, Exception> _handleException;

        public BinanceTimerProviderTests()
        {
            var method = typeof(BinanceTimerProvider)
                             .GetMethod("HandleException", BindingFlags.NonPublic | BindingFlags.Instance)
                         ?? throw new Exception($"Expected method HandleException on {nameof(BinanceTimerProvider)}");
            _handleException = (instance, exception) => method.Invoke(instance, new object[] { exception });
        }

        [Fact]
        public void GenericExceptionDoesNotExit()
        {
            var counterField = typeof(BinanceTimerProvider)
                .GetField("_consecutiveExceptions", BindingFlags.Instance | BindingFlags.NonPublic);

            var env = new TestEnvironment();
            SetEnv(env);

            var timer = new BinanceTimerProvider(new LoggerFactory(), new TestCandleDispenser());
            var countPre = counterField.GetValue(timer);

            Assert.Equal(0, countPre);
            _handleException(timer, new Exception());
            Assert.False(env.HasExited);

            var countPost = counterField.GetValue(timer);
            Assert.Equal(1, countPost);
        }

        [Fact]
        public void OrderRefusedExceptionExits()
        {
            var env = new TestEnvironment();
            SetEnv(env);
            var timer = new BinanceTimerProvider(new LoggerFactory(), new TestCandleDispenser());
            _handleException(timer, new OrderRefusedException());
            Assert.True(env.HasExited);
            Assert.Equal(env.Code, (int)ExitCode.OrderFailure);
        }

        [Fact]
        public void OutOfFundsExceptionExits()
        {
            var env = new TestEnvironment();
            SetEnv(env);
            var timer = new BinanceTimerProvider(new LoggerFactory(), new TestCandleDispenser());
            _handleException(timer, new OutOfFundsException());
            Assert.True(env.HasExited);
            Assert.Equal(env.Code, (int)ExitCode.OrderFailure);
        }

        [Fact]
        public void NestedExceptionsAreUnpacked()
        {
            var env = new TestEnvironment();
            SetEnv(env);
            var timer = new BinanceTimerProvider(new LoggerFactory(), new TestCandleDispenser());
            var exception = new Exception(string.Empty, new OrderRefusedException());
            _handleException(timer, exception);
            Assert.True(env.HasExited);
            Assert.Equal(env.Code, (int)ExitCode.OrderFailure);
        }
    }
}