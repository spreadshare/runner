using SpreadShare.Models;

namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Enumeration of exit codes.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Successful termination
        /// </summary>
        Success = 0,

        /// <summary>
        /// This status indicated a general error
        /// </summary>
        GeneralError = 1,

        /// <summary>
        /// Application was interrupted by Bash
        /// </summary>
        GeneralInterrupt = 2,

        /// <summary>
        /// Database was unavailable, this is often caused by the program running outside the docker container
        /// </summary>
        DatabaseUnreachable = 3,

        /// <summary>
        /// The configuration file was invalid, more detailed information can be found in the logs
        /// </summary>
        InvalidConfiguration = 4,

        /// <summary>
        /// The user instructed a shutdown via an exit command.
        /// </summary>
        UserShutdown = 5,

        /// <summary>
        /// The given command line arguments where not valid, see <see cref="CommandLineArgs"/>
        /// </summary>
        InvalidCommandLineArguments = 6,

        /// <summary>
        /// An algorithm failed to be stopped, the program terminated to prevent further risk.
        /// </summary>
        AlgorithmNotStopping = 7,

        /// <summary>
        /// An algorithm could not be started, the program terminated to prevent further risk.
        /// </summary>
        AlgorithmStartupFailure = 8,
    }
}