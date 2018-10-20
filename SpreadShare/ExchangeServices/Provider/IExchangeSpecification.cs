namespace SpreadShare.ExchangeServices.Provider
{
    /// <summary>
    /// Forces classes to specify the used exchange.
    /// </summary>
    internal interface IExchangeSpecification
    {
        /// <summary>
        /// Gets the type of exchange
        /// </summary>
        /// <returns>Type of the exchange used</returns>
        Exchange GetExchangeType();
    }
}
