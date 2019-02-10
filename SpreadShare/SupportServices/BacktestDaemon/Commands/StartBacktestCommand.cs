using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommandLine;
using SpreadShare.Algorithms;
using SpreadShare.ExchangeServices;
using SpreadShare.Models.Exceptions;
using SpreadShare.SupportServices.BacktestDaemon.CommandAttributes;
using SpreadShare.SupportServices.Configuration;
using SpreadShare.Utilities;

#pragma warning disable SA1402

namespace SpreadShare.SupportServices.BacktestDaemon.Commands
{
    /// <summary>
    /// Command that start an algorithm.
    /// </summary>
    [CommandName("run")]
    [CommandArguments(typeof(StartBacktestCommandArguments))]
    [CommandDescription("run an algorithm")]
    internal class StartBacktestCommand : BacktestCommand
    {
        private readonly Type _algo;
        private readonly AlgorithmConfiguration _configuration;
        private StartBacktestCommandArguments _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartBacktestCommand"/> class.
        /// </summary>
        /// <param name="inputs">inputs.</param>
        public StartBacktestCommand(string[] inputs)
            : base(inputs)
        {
            Parser parser = new Parser(x => x.HelpWriter = null);
            parser.ParseArguments<StartBacktestCommandArguments>(inputs)
                .WithNotParsed(_ => throw new InvalidCommandException("invalid arguments, use help to get more info"))
                .WithParsed(x => _args = x);

            // Check if the input type is a valid algorithm
            _algo = Reflections.GetAllImplementations(typeof(IBaseAlgorithm))
                .FirstOrDefault(x => x.Name == _args.AlgorithmName)
                    ?? throw new InvalidCommandException($"{_args.AlgorithmName} is not a known algorithm");

            // Retrieve the settings type
            var settingsType = Reflections.GetAllSubtypes(typeof(AlgorithmConfiguration))
                            .FirstOrDefault(s => Reflections.AlgorithmMatchesConfiguration(_algo, s))
                        ?? throw new InvalidCommandException(
                            $"{_args.AlgorithmName} does not have a settings object and cannot be started.");

            // Optionally load with custom path.
            _args.ConfigurationPath = _args.ConfigurationPath ?? _args.AlgorithmName + ".yaml";
            _configuration = ConfigurationLoader.LoadConfiguration(settingsType, _args.ConfigurationPath);

            if (_configuration.Exchange == Exchange.Backtesting)
            {
                var (begin, end) = DatabaseUtilities.Instance.GetTimeStampEdges(_configuration.TradingPairs);
                Configuration.Configuration.Instance.BacktestSettings.BeginTimeStamp = begin;
                Configuration.Configuration.Instance.BacktestSettings.EndTimeStamp = end;
                Program.CommandLineArgs.BacktestOutputPath = _args.OutputPath;
            }
        }

        /// <inheritdoc/>
        public override void Execute(BacktestDaemonState state)
        {
            var startStr = DateTimeOffset
                .FromUnixTimeMilliseconds(Configuration.Configuration.Instance.BacktestSettings.BeginTimeStamp)
                .ToString(CultureInfo.InvariantCulture);
            var endStr = DateTimeOffset
                .FromUnixTimeMilliseconds(Configuration.Configuration.Instance.BacktestSettings.EndTimeStamp)
                .ToString(CultureInfo.InvariantCulture);

            Console.WriteLine($"Starting backtest for {_algo.Name} from {startStr} to {endStr}");

            // Set custom id or increment.
            state.CurrentBacktestID = _args.ID == -1
                ? state.CurrentBacktestID + 1
                : _args.ID;
            state.CurrentBacktestConfigurationPath = _args.ConfigurationPath;

            state.AllocationManager.SetInitialConfiguration(new Dictionary<Exchange, Dictionary<Type, decimal>>()
            {
                {
                    Exchange.Backtesting,
                    new Dictionary<Type, decimal> { { _algo, 1 } }
                },
                {
                    Exchange.Binance,
                    new Dictionary<Type, decimal> { { _algo, 1 } }
                },
            });

            var result = state.AlgorithmService.StartAlgorithm(_algo, _configuration);

            if (!result.Success)
            {
                Console.WriteLine($"Cannot start algorithm -> {result.Message}");
            }
        }
    }

    /// <summary>
    /// The arguments object for this command.
    /// </summary>
    internal class StartBacktestCommandArguments
    {
        /// <summary>
        /// Gets or sets name of the algorithm to start.
        /// </summary>
        [Option("algorithm", Required = true)]
        public string AlgorithmName { get; set; }

        /// <summary>
        /// Gets or sets the path of the configuration.
        /// </summary>
        [Option("config")]
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// Gets or sets the output path of the backtest.
        /// </summary>
        [Option("output")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the ID of the backtest.
        /// </summary>
        [Option("id", Default = -1)]
        public int ID { get; set; }
    }
}

#pragma warning restore SA1402