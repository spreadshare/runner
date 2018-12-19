using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using CryptoExchange.Net.Objects;
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
        /// <param name="method"></param>
        /// <param name="logger"></param>
        /// <param name="maxRetries"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResponseObject<T> RetryMethod<T>(Func<ResponseObject<T>> method, ILogger logger, int maxRetries = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var result = method();
                if (result.Success)
                {
                    return result;
                }

                logger.LogWarning($"{result.Message} - attempt {i+1}/{maxRetries} for method {method.Method.Name}");
            }

            return new ResponseObject<T>(ResponseCode.Error);
        }
        
    }
}