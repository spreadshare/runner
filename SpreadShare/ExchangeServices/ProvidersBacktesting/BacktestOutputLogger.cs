using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpreadShare.Models.Database;
using SpreadShare.SupportServices;
using static System.IO.File;

namespace SpreadShare.ExchangeServices.ProvidersBacktesting
{
    /// <summary>
    /// Logs trades and state switches to output folder.
    /// </summary>
    internal class BacktestOutputLogger
    {
        private const char Delimiter = '|';
        private BacktestTimerProvider _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutputLogger"/> class.
        /// </summary>
        /// <param name="databaseContext">DatabaseContext to fetch trades and switches.</param>
        /// <param name="timer">BacktestTimerProvider to get timespan information.</param>
        /// <param name="outputFolder">General backtest output folder.</param>
        public BacktestOutputLogger(DatabaseContext databaseContext, BacktestTimerProvider timer, string outputFolder)
        {
            DatabaseContext = databaseContext;
            OutputFolder = outputFolder;
            _timer = timer;
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        private DatabaseContext DatabaseContext { get; }

        /// <summary>
        /// Gets or sets the output folder.
        /// </summary>
        private string OutputFolder { get; set; }

        /// <summary>
        /// Log output (trade and state switches) to output file.
        /// </summary>
        public void Output()
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
            OutputConfiguration(Path.Combine(OutputFolder, "appsettings.json"));

            // Output trades
            OutputTrades(Path.Combine(OutputFolder, "trades.csv"));

            // Output state switches
            OutputStateSwitches(Path.Combine(OutputFolder, "state_switches.csv"));

            // Output timespan
            OutputTimespan(Path.Combine(OutputFolder, "timespan.json"));
        }

        /// <summary>
        /// Output appsettings.json (without credentials) to output folder.
        /// </summary>
        /// <param name="filepath">Filepath to store configuration at.</param>
        private static void OutputConfiguration(string filepath)
        {
            string rawjson = ReadAllText(Program.CommandLineArgs.ConfigurationPath);
            rawjson = Regex.Replace(rawjson, "Password=.+?\\;", "Password=[...];");

            JObject configuration = JObject.Parse(rawjson);

            configuration["BinanceClientSettings"]["Credentials"]["Key"] = "api-key";
            configuration["BinanceClientSettings"]["Credentials"]["Secret"] = "api-secret";

            WriteAllText(filepath, configuration.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Output all executed trades to filepath.
        /// </summary>
        /// <param name="filepath">Filepath to store trades at.</param>
        private void OutputTrades(string filepath)
        {
            var builder = new StringBuilder();
            builder.AppendLine(DatabaseTrade.GetStaticCsvHeader(Delimiter));

            foreach (var trade in DatabaseContext.Trades.OrderBy(x => x.FilledTimestamp).ThenBy(x => x.TradeId))
            {
                builder.AppendLine(trade.GetCsvRepresentation(Delimiter));
            }

            WriteAllText(filepath, builder.ToString());
        }

        /// <summary>
        /// Output all executed state switches to filepath.
        /// </summary>
        /// <param name="filepath">Filepath to store trades at.</param>
        private void OutputStateSwitches(string filepath)
        {
            var builder = new StringBuilder();
            builder.AppendLine(StateSwitchEvent.GetStaticCsvHeader(Delimiter));
            foreach (var stateSwitch in DatabaseContext.StateSwitchEvents.OrderBy(x => x.Timestamp))
            {
                builder.AppendLine(stateSwitch.GetCsvRepresentation(Delimiter));
            }

            WriteAllText(filepath, builder.ToString());
        }

        private void OutputTimespan(string filepath)
        {
            string data = JsonConvert.SerializeObject(new
            {
                BeginTime = _timer.BeginTime.ToUnixTimeMilliseconds(),
                EndTime = _timer.EndTime.ToUnixTimeMilliseconds()
            });

            WriteAllText(filepath, data);
        }
    }
}
