using System;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Collection of helper methods
    /// </summary>
    internal static class HelperMethods
    {
        /// <summary>
        /// Retry a ReponseObject method a number of times.
        /// </summary>
        /// <param name="method">The method to retry</param>
        /// <param name="logger">Logger to report to</param>
        /// <param name="maxRetries">Maximum number of retries (default 5)</param>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <returns>First (if any) success response of <see pref="method"/></returns>
        public static ResponseObject<T> RetryMethod<T>(Func<ResponseObject<T>> method, ILogger logger, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var result = method();
                if (result.Success)
                {
                    return result;
                }

                logger.LogWarning($"{result.Message} - attempt {i + 1}/{maxRetries} for method {method.Method.Name}");
            }

            return new ResponseObject<T>(ResponseCode.Error);
        }
    }
}