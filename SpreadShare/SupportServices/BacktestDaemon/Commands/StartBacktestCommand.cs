using System;
using System.Globalization;
using System.Linq;
using CommandLine;
using SpreadShare.Algorithms;
using SpreadShare.Models;
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
                            $"{_args.AlgorithmName} does not have a configuration object and cannot be started.");

            // Optionally load with custom path.
            _args.ConfigurationPath = _args.ConfigurationPath ?? _args.AlgorithmName + ".yaml";

            try
            {
                _configuration = ConfigurationLoader.LoadConfiguration(settingsType, _args.ConfigurationPath);
            }
            catch (Exception e)
            {
                throw new InvalidCommandException(e.Message);
            }

            foreach (var pair in _configuration.TradingPairs)
            {
                if (!DatabaseUtilities.Instance.ValidateCandleWidth(
                    pair,
                    Configuration.Configuration.Instance.CandleWidth))
                {
                    throw new InvalidConfigurationException(
                        $"Database candle interval for {pair} is not compatible with {Configuration.Configuration.Instance.CandleWidth}");
                }
            }

            ConfigureTimestampEdges(BacktestDaemonService.Instance.State, _args);
            Program.CommandLineArgs.BacktestOutputPath = _args.OutputPath;
        }

        /// <inheritdoc/>
        public override void Execute(BacktestDaemonState state)
        {
            var startStr = DateTimeOffset
                .FromUnixTimeMilliseconds(BacktestDaemonService.Instance.State.BeginTimeStamp)
                .ToString(CultureInfo.InvariantCulture);
            var endStr = DateTimeOffset
                .FromUnixTimeMilliseconds(BacktestDaemonService.Instance.State.EndTimeStamp)
                .ToString(CultureInfo.InvariantCulture);

            Configuration.Configuration.Instance.EnabledAlgorithm.ChangeAlgorithmConfiguration(_algo, _configuration);

            Console.WriteLine($"Starting backtest for {_algo.Name} from {startStr} to {endStr}");
            Console.WriteLine($"Configured Candle Width: {Configuration.Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration.CandleWidth}m");

            // Set custom id or increment.
            state.CurrentBacktestID = _args.ID == -1
                ? state.CurrentBacktestID + 1
                : _args.ID;
            state.CurrentBacktestConfigurationPath = _args.ConfigurationPath;

            // Reset the allocation for a new run.
            state.AllocationManager.SetInitialConfiguration(Configuration.Configuration.Instance.EnabledAlgorithm
                .Allocation);

            // Backtests are run synchronously by design.
            var result = state.AlgorithmService.StartAlgorithm(_algo, _configuration);

            if (result.Success || result.Message == ResponseObject.OutOfFunds.Message)
            {
                // Notify third party applications that the backtest with their id has finished.
                Console.WriteLine($"BACKTEST_FINISHED={BacktestDaemonService.Instance.State.CurrentBacktestID}");
            }
            else
            {
                Console.WriteLine($"Algorithm Execution failed -> {result.Message}");
                Console.WriteLine($"BACKTEST_FAILED={BacktestDaemonService.Instance.State.CurrentBacktestID}");
            }
        }

        private void ConfigureTimestampEdges(BacktestDaemonState state, StartBacktestCommandArguments args)
        {
            var (begin, end) = DatabaseUtilities.Instance.GetTimeStampEdges(_configuration.TradingPairs);

            if (args.BeginEpoch != 0)
            {
                if (args.BeginEpoch < 0)
                {
                    throw new InvalidCommandException(
                        $"Cannot set begin epoch to {DateTimeOffset.FromUnixTimeMilliseconds(args.BeginEpoch)}, " +
                        $"the database starts at {DateTimeOffset.FromUnixTimeMilliseconds(begin)}");
                }

                if (args.EndEpoch < begin)
                {
                    throw new InvalidCommandException("end_epoch was smaller than database start.");
                }

                if (args.BeginEpoch < begin + TimeSpan.FromDays(14).TotalMilliseconds)
                {
                    Console.WriteLine($"WARNING: Custom Begin Epoch is close to the beginning of the data, be carefull" +
                                      $"not to read to far into the past.");
                }

                state.BeginTimeStamp = begin;
            }
            else
            {
                state.BeginTimeStamp = begin + (long)TimeSpan.FromDays(14).TotalMilliseconds;
            }

            if (args.EndEpoch != 0)
            {
                if (args.EndEpoch > end)
                {
                    throw new InvalidCommandException(
                        $"Cannot set end epoch to {DateTimeOffset.FromUnixTimeMilliseconds(args.EndEpoch)}, " +
                        $"the database stops at {DateTimeOffset.FromUnixTimeMilliseconds(end)}");
                }

                end = args.EndEpoch;
            }

            state.EndTimeStamp = end - (long)TimeSpan.FromMinutes(_configuration.CandleWidth).TotalMilliseconds;
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
        /// Gets or sets the begin epoch argument of a backtest.
        /// </summary>
        [Option("begin_epoch")]
        public long BeginEpoch { get; set; }

        /// <summary>
        /// Gets or sets the end epoch argument of a backtest.
        /// </summary>
        [Option("end_epoch")]
        public long EndEpoch { get; set; }

        /// <summary>
        /// Gets or sets the ID of the backtest.
        /// </summary>
        [Option("id", Default = -1)]
        public int ID { get; set; }
    }
}

#pragma warning restore SA1402