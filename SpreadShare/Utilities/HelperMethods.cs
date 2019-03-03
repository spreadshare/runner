using System;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using SpreadShare.Models;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Collection of helper methods.
    /// </summary>
    internal static class HelperMethods
    {
        /// <summary>
        /// Retry a ResponseObject method a number of times.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to report to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <param name="backoffMillis">The ground number for the exponential backoff delay.</param>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <returns>First (if any) success response of <see pref="method"/>.</returns>
        public static ResponseObject<T> RetryMethod<T>(Func<ResponseObject<T>> method, ILogger logger, int maxRetries = 5, int backoffMillis = 200)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(maxRetries).NotZero().NotNegative();
            Guard.Argument(backoffMillis).NotNegative();

            for (int i = 0; i < maxRetries; i++)
            {
                var result = method();
                if (result.Success)
                {
                    return result;
                }

                var backoff = TimeSpan.FromMilliseconds(backoffMillis * Math.Pow(2, i + 1));
                Thread.Sleep((int)backoff.TotalMilliseconds);

                logger?.LogWarning($"{result.Message} - attempt {i + 1}/{maxRetries} for method {method.Method.Name}");
            }

            return new ResponseObject<T>(ResponseCode.Error);
        }

        /// <summary>
        /// Retry a Response object method a number of times.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to report to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <param name="backoffMillis">The ground number for the exponential backoff delay.</param>
        /// <returns>First (if any) success response of the <see pref="method"/>.</returns>
        public static ResponseObject RetryMethod(Func<ResponseObject> method, ILogger logger, int maxRetries = 5, int backoffMillis = 200)
        {
            var query = RetryMethod<string>(method, logger, maxRetries, backoffMillis);
            return new ResponseObject(query.Code);
        }

        /// <summary>
        /// A division that returns zero if the denominator is zero.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns>safe division result.</returns>
        public static decimal SafeDiv(decimal numerator, decimal denominator)
        {
            return denominator == 0 ? 0 : numerator / denominator;
        }
    }
}