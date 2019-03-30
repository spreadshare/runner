using System;
using Dawn;

namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Type of observer who's functionality can be set a construction time.
    /// </summary>
    /// <typeparam name="T">Type of data to observe.</typeparam>
    internal class ConfigurableObserver<T> : IObserver<T>
    {
        private readonly Action _actionOnCompleted;
        private readonly Action<Exception> _actionOnError;
        private readonly Action<T> _actionOnNext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurableObserver{T}"/> class.
        /// </summary>
        /// <param name="onCompleted">Callback for observing end of stream.</param>
        /// <param name="onError">Callback for observing errors.</param>
        /// <param name="onNext">Callback for observing new data.</param>
        public ConfigurableObserver(Action onCompleted, Action<Exception> onError, Action<T> onNext)
        {
            Guard.Argument(onNext).NotNull();
            Guard.Argument(onCompleted).NotNull();
            Guard.Argument(onError).NotNull();

            _actionOnCompleted = OnCompleted;
            _actionOnError = onError;
            _actionOnNext = onNext;
        }

        /// <inheritdoc />
        public void OnCompleted() => _actionOnCompleted();

        /// <inheritdoc />
        public void OnError(Exception error) => _actionOnError(error);

        /// <inheritdoc />
        public void OnNext(T value) => _actionOnNext(value);
    }
}