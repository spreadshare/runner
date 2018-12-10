using System;
using Dawn;

namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Type of observer who's functionality can be set a construction time
    /// </summary>
    /// <typeparam name="T">Type of data to observe</typeparam>
    internal class ConfigurableObserver<T> : IObserver<T>
    {
        private readonly Action<T> _actionOnNext;
        private readonly Action _actionOnCompleted;
        private readonly Action<Exception> _actionOnError;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurableObserver{T}"/> class.
        /// </summary>
        /// <param name="onNext">Callback for observing new data</param>
        /// <param name="onCompleted">Callback for observing end of stream</param>
        /// <param name="onError">Callback for observing errors</param>
        public ConfigurableObserver(Action<T> onNext, Action onCompleted, Action<Exception> onError)
        {
            Guard.Argument(onNext).NotNull();
            Guard.Argument(onCompleted).NotNull();
            Guard.Argument(onError).NotNull();
            _actionOnCompleted = OnCompleted;
            _actionOnNext = onNext;
            _actionOnError = onError;
        }

        /// <inheritdoc />
        public void OnCompleted() => _actionOnCompleted();

        /// <inheritdoc />
        public void OnError(Exception error) => _actionOnError(error);

        /// <inheritdoc />
        public void OnNext(T value) => _actionOnNext(value);
    }
}