using System;

namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Proxy for the build in Environment object.
    /// </summary>
    internal class RealEnvironment : IEnv
    {
        /// <inheritdoc />
        public void ExitEnvironment(int code)
        {
            Environment.Exit(code);
        }
    }
}