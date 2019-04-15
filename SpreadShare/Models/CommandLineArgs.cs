using CommandLine;

namespace SpreadShare.Models
{
    /// <summary>
    /// Container for all parsed command line arguments.
    /// </summary>
    internal class CommandLineArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the --trading flag was used, mutually exclusive with --backtest.
        /// </summary>
        [Option('t', "trading", Default = false, Required = true, SetName= "trading", HelpText = "Needs to be enabled to actually perform trades.")]
        public bool Trading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the --backtesting flag used. mutually exclusive with --trading.
        /// </summary>
        [Option('b', "backtesting", Default = false, Required = true, SetName = "backtesting", HelpText = "Enable the backtest daemon.")]
        public bool Backtesting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the --migrate flag was used. mutually exclusive with --trading and --backtest.
        /// </summary>
        [Option('m', "migrate", Default = false, Required = true, SetName = "migrate", HelpText = "Ensure the database is migrated, then shut down.")]
        public bool Migrate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip the database. Mutually exclusive with --migrate.
        /// </summary>
        [Option('s', "skipdb", Default = false, HelpText = "Skip all database calls")]
        public bool SkipDatabase { get; set; }

        /// <summary>
        /// Gets or sets the filepath of the configuration JSON.
        /// </summary>
        [Option("configpath", Default = "appsettings.yaml", HelpText = "The path to the configuration.yaml file")]
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application should have high verbosity.
        /// </summary>
        [Option('v', "verbose", Default = false, HelpText = "The verbosity of the logger")]
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// Gets or sets the output path for the backtesting result.
        /// </summary>
        [Option("backtestpath", Default = "", HelpText = "The output of the backtest run")]
        public string BacktestOutputPath { get; set; }
    }
}