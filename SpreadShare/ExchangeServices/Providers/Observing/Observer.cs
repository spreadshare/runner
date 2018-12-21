using System;
using System.Linq.Expressions;

namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Observer class that allows implementations to only override the required
    /// functions (<see cref="OnCompleted"/>, <see cref="OnError"/>, <see cref="OnNext"/>).
    /// </summary>
    /// <typeparam name="T">Type of the data returned to the observer.</typeparam>
    internal abstract class Observer<T> : IObserver<T>
    {
        private readonly IDisposable _unsubscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="Observer{T}"/> class.
        /// </summary>
        /// <param name="observable"><see cref="Observable{T}"/> to subscribe to.</param>
        protected Observer(IObservable<T> observable) => _unsubscriber = observable.Subscribe(this);

        /// <inheritdoc />
        public virtual void OnCompleted() => Expression.Empty();

        /// <inheritdoc />
        public virtual void OnError(Exception error) => Expression.Empty();

        /// <inheritdoc />
        public virtual void OnNext(T value) => Expression.Empty();

        /// <summary>
        /// Unsubscribe from the <see cref="Observable{T}"/>.
        /// </summary>
        protected void Unsubscribe()
        {
            _unsubscriber.Dispose();
        }
    }
}
