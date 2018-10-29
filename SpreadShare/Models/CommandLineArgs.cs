using CommandLine;

namespace SpreadShare.Models
{
    /// <summary>
    /// Container for all parsed command line arguments
    /// </summary>
    internal class CommandLineArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the --trading flag was used.
        /// </summary>
        [Option("trading", Default = false, HelpText = "Needs to be enabled to actually perform trades.")]
        public bool Trading { get; set; }
    }
}