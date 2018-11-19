using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestOutputLogger"/> class.
        /// </summary>
        /// <param name="databaseContext">DatabaseContext to fetch trades and switches</param>
        /// <param name="outputFolder">General backtest output folder</param>
        public BacktestOutputLogger(DatabaseContext databaseContext, string outputFolder)
        {
            DatabaseContext = databaseContext;
            OutputFolder = outputFolder;
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
            OutputFolder = Path.Combine(OutputFolder, $"Backtest_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss}");

            // Create directory
            Directory.CreateDirectory(OutputFolder);

            // Copy configuration
            OutputConfiguration(Path.Combine(OutputFolder, "configuration.json"));

            // Output trades
            OutputTrades(Path.Combine(OutputFolder, "trades.csv"));
        }

        /// <summary>
        /// Output appsettings.json (without credentials) to output folder.
        /// </summary>
        /// <param name="filepath">Filepath to store configuration at</param>
        private static void OutputConfiguration(string filepath)
        {
            string configuration = ReadAllText("appsettings.json");
            configuration = Regex.Replace(configuration, ".*\"Key\":.*", "            \"Key\": \"api_key\",");
            configuration = Regex.Replace(configuration, ".*\"Secret\":.*", "            \"Secret\": \"api_secret\",");
            WriteAllText(filepath, configuration);
        }

        /// <summary>
        /// Output all executed trades to filepath.
        /// </summary>
        /// <param name="filepath">Filepath to store trades at</param>
        private void OutputTrades(string filepath)
        {
            var builder = new StringBuilder();
            builder.AppendLine(DatabaseTrade.GetCsvHeader());
            foreach (var trade in DatabaseContext.Trades)
            {
                builder.AppendLine(trade.ToString());
            }

            WriteAllText(filepath, builder.ToString());
        }
    }
}
