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
        /// This status code is not in use
        /// </summary>
        Undefined = 1,

        /// <summary>
        /// An algorithm failed to be stopped, the program terminated to prevent further risk.
        /// </summary>
        AlgorithmNotStopping = 2,

        /// <summary>
        /// Database was unavailable, this is often caused by the program running outside the docker container
        /// </summary>
        DatabaseUnreachable = 3,

        /// <summary>
        /// The configuration file was invalid, more detailed information can be found in the logs
        /// </summary>
        InvalidConfiguration = 4,
    }
}