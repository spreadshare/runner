using System;
using System.Collections.Generic;

namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Observable object that can give an update to observers.
    /// </summary>
    /// <typeparam name="T">Type of the observable</typeparam>
    internal abstract class Observable<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> _observers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Observable{T}"/> class.
        /// </summary>
        protected Observable() => _observers = new List<IObserver<T>>();

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer);
        }

        /// <summary>
        /// Update all observers with new data.
        /// </summary>
        /// <param name="data">New data</param>
        protected void UpdateObservers(T data)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(data);
            }
        }

        /// <summary>
        /// Defines class for IObservers to unsubscribe from the IObservable.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Unsubscriber"/> class.
            /// </summary>
            /// <param name="observers">Observers list to remove observer from</param>
            /// <param name="observer">Observer that wants to unsubscribe</param>
            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_observer != null)
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
