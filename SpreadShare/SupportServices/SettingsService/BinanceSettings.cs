namespace SpreadShare.SupportServices.SettingsService
{
    /// <summary>
    /// Settings for the Binance exchange
    /// </summary>
    public class BinanceSettings
    {
        /// <summary>
        /// Gets the credentials for Binance
        /// </summary>
        public BinanceCredentials Credentials { get; set; }

        /// <summary>
        /// Gets the receive window for Binance
        /// </summary>
        public long ReceiveWindow { get; set; }
    }
}