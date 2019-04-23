namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Interface for an environment.
    /// </summary>
    public interface IEnv
    {
        /// <summary>
        /// Exit the environment.
        /// </summary>
        /// <param name="code">The code to exit with.</param>
        void ExitEnvironment(int code);
    }
}