using System;
using System.Collections.Generic;
using SpreadShare.Algorithms.Implementations;
using SpreadShare.ExchangeServices.ProvidersBacktesting;
using SpreadShare.Models.Database;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// Abstract definition for a command that gets the indicator over a set of candles.
    /// </summary>
    /// <typeparam name="T">The return type of the indicator.</typeparam>
    [CommandArguments("pair", "epoch", "n")]
    [CommandDescription("get the RelativeStrengthIndex of [pair] at [epoch] of [n] candles.")]
    internal abstract class IndicatorCommand<T> : BacktestCommand
    {
        private readonly Func<IEnumerable<BacktestingCandle>, T> _indicator;
        private readonly TradingPair _pair;
        private readonly long _epoch;
        private readonly int _n;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndicatorCommand{T}"/> class.
        /// </summary>
        /// <param name="indicator">The indicator method.</param>
        /// <param name="inputs">The candles to use indicator over.</param>
        protected IndicatorCommand(Func<IEnumerable<BacktestingCandle>, T> indicator, params string[] inputs)
        {
            _indicator = indicator;
            if (inputs.Length != 4)
            {
                throw new InvalidCommandException($"{GetType().Name} needs exactly 3 arguments");
            }

            try
            {
                _pair = TradingPair.Parse(inputs[1]);
            }
            catch (Exception)
            {
                throw new InvalidCommandException($"{inputs[1]} is not a valid trading pair.");
            }

            if (!(long.TryParse(inputs[2], out _epoch) && DateTimeOffset.FromUnixTimeMilliseconds(_epoch) != null))
            {
                throw new InvalidCommandException($"{_epoch} is not a valid ms epoch");
            }

            if (!(int.TryParse(inputs[3], out _n) && _n > 0))
            {
                throw new InvalidCommandException($"{_n} is not a valid value for n");
            }
        }

        /// <inheritdoc />
        public override void Execute(BacktestDaemonState state)
        {
            var config = new TemplateAlgorithmConfiguration(new[] { _pair }, Configuration.Configuration.Instance.CandleWidth);
            var container = state.ExchangeFactory.BuildBacktestingContainer<TemplateAlgorithm>(config);
            var timer = (BacktestTimerProvider)container.TimerProvider;
            timer.SetCurrentTime(DateTimeOffset.FromUnixTimeMilliseconds(_epoch));
            var candles = container.DataProvider.GetCandles(_pair, _n);
            Console.WriteLine(_indicator(candles));
        }
    }
}