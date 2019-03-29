namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Observable with a public Publish endpoint.
    /// </summary>
    /// <typeparam name="T">The type of the observable.</typeparam>
    internal class ConfigurableObservable<T> : Observable<T>
    {
        /// <summary>
        /// Published updates to the observers.
        /// </summary>
        /// <param name="data">The data to publish.</param>
        public void Publish(T data)
            => UpdateObservers(data);
    }
}