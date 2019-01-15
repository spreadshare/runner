using System;
using System.Threading.Tasks;

namespace SpreadShare.ExchangeServices.Providers
{
    /// <summary>
    /// Exchange implementation of the TimerProvider.
    /// </summary>
    internal class ExchangeTimerProvider : TimerProvider
    {
        /// <inheritdoc />
        public override DateTimeOffset CurrentTime => DateTimeOffset.Now;

        /// <summary>
        /// Notifies the observer periodically.
        /// </summary>
        public override async void RunPeriodicTimer()
        {
            while (true)
            {
                UpdateObservers(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                await Task.Delay(2000).ConfigureAwait(false);
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}