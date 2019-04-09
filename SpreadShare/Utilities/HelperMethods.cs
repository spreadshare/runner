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
        /// Retry a ResponseObject method a number of times, with the ability to mutate the retry context.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to report to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <param name="backoffMillis">The ground number for the exponential backoff delay. (can be mutated).</param>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <returns>First (if any) success response of <see pref="method"/>.</returns>
        public static ResponseObject<T> RetryMethod<T>(Func<RetryContext, ResponseObject<T>> method, ILogger logger, int maxRetries = 5, int backoffMillis = 200)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(maxRetries).NotZero().NotNegative();
            Guard.Argument(backoffMillis).NotNegative();

            int backoffTries = 0;
            var context = new RetryContext();

            for (var i = 0; i < maxRetries; i++)
            {
                var result = method(context);
                if (result.Success)
                {
                    return result;
                }

                if (!context.BackoffDisabled)
                {
                    var backoff = TimeSpan.FromMilliseconds(backoffMillis * Math.Pow(2, backoffTries + 1));
                    Thread.Sleep((int)backoff.TotalMilliseconds);
                    backoffTries++;
                }

                // Assume that backoff should be used unless disabled again.
                context.EnableBackoff();

                context.Iteration++;
                logger?.LogWarning($"{result.Message} - attempt {i + 1}/{maxRetries} for method {method.Method.Name}");
            }

            return new ResponseObject<T>(ResponseCode.Error);
        }

        /// <summary>
        /// Retry a method a certain number of time.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">To create output.</param>
        /// <param name="maxRetries">Maximum number of retries.</param>
        /// <param name="backoffMillis">Ground number for the exponential backoff.</param>
        /// <typeparam name="T">The type that is contained in the response object.</typeparam>
        /// <returns>First (if any) success response.</returns>
        public static ResponseObject<T> RetryMethod<T>(
            Func<ResponseObject<T>> method,
            ILogger logger,
            int maxRetries = 5,
            int backoffMillis = 200)
            => RetryMethod(_ => method(), logger, maxRetries, backoffMillis);

        /// <summary>
        /// Retry a Response object method a number of times.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to report to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <param name="backoffMillis">The ground number for the exponential backoff delay.</param>
        /// <returns>First (if any) success response of the <see pref="method"/>.</returns>
        public static ResponseObject RetryMethod(Func<RetryContext, ResponseObject> method, ILogger logger, int maxRetries = 5, int backoffMillis = 200)
        {
            var query = RetryMethod<string>(method, logger, maxRetries, backoffMillis);
            return new ResponseObject(query.Code);
        }

        /// <summary>
        /// Retry a ResponseObject method a number of times, with the ability to mutate the retry context.
        /// </summary>
        /// <param name="method">The method to retry.</param>
        /// <param name="logger">Logger to report to.</param>
        /// <param name="maxRetries">Maximum number of retries (default 5).</param>
        /// <param name="backoffMillis">The ground number for the exponential backoff delay. (can be mutated).</param>
        /// <returns>First (if any) success response of <see pref="method"/>.</returns>
        public static ResponseObject RetryMethod(
            Func<ResponseObject> method,
            ILogger logger,
            int maxRetries = 5,
            int backoffMillis = 200)
            => RetryMethod(_ => method(), logger, maxRetries, backoffMillis);

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

        /// <summary>
        /// Get the most inner exception.
        /// </summary>
        /// <param name="e">The exception to unpack.</param>
        /// <returns>the most inner exception.</returns>
        public static Exception Unpack(this Exception e)
        {
            if (e.InnerException != null)
            {
                return e.InnerException.Unpack();
            }

            return e;
        }

        /// <summary>
        /// Converts a datetime object to unix timestamp.
        /// </summary>
        /// <param name="dateTime">Date time object to convert.</param>
        /// <returns>Milliseconds since 01-01-1970.</returns>
        public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds();
        }
    }
}