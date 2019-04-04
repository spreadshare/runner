namespace SpreadShare.ExchangeServices.Providers.Observing
{
    /// <summary>
    /// Observable that exposes a public publish endpoint.
    /// </summary>
    /// <typeparam name="T">The observable data type.</typeparam>
    internal class ConfigurableObservable<T> : Observable<T>
    {
        /// <summary>
        /// Update the subscribers of this <see cref="ConfigurableObservable{T}"/>.
        /// </summary>
        /// <param name="data">The data to publish.</param>
        public void Publish(T data)
            => UpdateObservers(data);
    }
}