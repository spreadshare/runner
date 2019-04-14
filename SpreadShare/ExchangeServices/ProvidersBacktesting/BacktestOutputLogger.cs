using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SpreadShare.Models.Database;
using SpreadShare.SupportServices;
using SpreadShare.SupportServices.BacktestDaemon;
using static System.IO.File;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Logs trades and state switches to output folder.
    /// </summary>
    internal class BacktestOutputLogger
    {
        private const char Delimiter = '|';
        private readonly BacktestTimerProvider _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutputLogger"/> class.
        /// </summary>
        /// <param name="timer">BacktestTimerProvider to get timespan information.</param>
        /// <param name="outputFolder">General backtest output folder.</param>
        public BacktestOutputLogger(BacktestTimerProvider timer, string outputFolder)
        {
            OutputFolder = outputFolder;
            _timer = timer;
        }

        /// <summary>
        /// Gets or sets the output folder.
        /// </summary>
        private string OutputFolder { get; set; }

        /// <summary>
        /// Log output (trade and state switches) to output file.
        /// </summary>
        /// <param name="orders">Orders that have been traded.</param>
        /// <param name="stateSwitchEvents">List of state switches during the backtest.</param>
        public void Output(List<BacktestOrder> orders, List<StateSwitchEvent> stateSwitchEvents)
        {
            // Set name of folder
            if (string.IsNullOrEmpty(Program.CommandLineArgs.BacktestOutputPath))
            {
                OutputFolder = Path.Combine(OutputFolder, $"Backtest_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss}");
            }
            else
            {
                OutputFolder = Path.Combine(OutputFolder, Program.CommandLineArgs.BacktestOutputPath);
            }

            // Create directory
            Directory.CreateDirectory(OutputFolder);

            // Copy configuration
            OutputConfiguration(Path.Combine(OutputFolder, "appsettings.yaml"));

            // Output trades
            OutputOrders(Path.Combine(OutputFolder, "trades.csv"), orders);

            // Output state switches
            OutputStateSwitches(Path.Combine(OutputFolder, "state_switches.csv"), stateSwitchEvents);

            // Output timespan
            OutputTimespan(Path.Combine(OutputFolder, "timespan.json"));
        }

        /// <summary>
        /// Output appsettings.json (without credentials) to output folder.
        /// </summary>
        /// <param name="filepath">Filepath to store configuration at.</param>
        private static void OutputConfiguration(string filepath)
        {
            Copy(BacktestDaemonService.Instance.State.CurrentBacktestConfigurationPath, filepath, true);
        }

        /// <summary>
        /// Output all executed orders to filepath.
        /// </summary>
        /// <param name="filepath">Filepath to store trades at.</param>
        /// <param name="orders">Orders that have been traded.</param>
        private static void OutputOrders(string filepath, List<BacktestOrder> orders)
        {
            var builder = new StringBuilder();
            builder.AppendLine(BacktestOrder.GetStaticCsvHeader(Delimiter));

            foreach (var order in orders)
            {
                builder.AppendLine(order.GetCsvRepresentation(Delimiter));
            }

            WriteAllText(filepath, builder.ToString());
        }

        /// <summary>
        /// Output all executed state switches to filepath.
        /// </summary>
        /// <param name="filepath">Filepath to store trades at.</param>
        /// <param name="stateSwitchEvents">List of state switches during the backtest.</param>
        private static void OutputStateSwitches(string filepath, List<StateSwitchEvent> stateSwitchEvents)
        {
            var builder = new StringBuilder();
            builder.AppendLine(StateSwitchEvent.GetStaticCsvHeader(Delimiter));
            foreach (var stateSwitch in stateSwitchEvents)
            {
                builder.AppendLine(stateSwitch.GetCsvRepresentation(Delimiter));
            }

            WriteAllText(filepath, builder.ToString());
        }

        /// <summary>
        /// Output the start and end timestamp.
        /// </summary>
        /// <param name="filepath">Filepath to store timestamps at.</param>
        private void OutputTimespan(string filepath)
        {
            string data = JsonConvert.SerializeObject(new
            {
                BeginTime = _timer.BeginTime.ToUnixTimeMilliseconds(),
                EndTime = _timer.EndTime.ToUnixTimeMilliseconds(),
            });

            WriteAllText(filepath, data);
        }
    }
}
