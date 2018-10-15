namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings for the Binance exchange
    /// </summary>
    public class BinanceSettings
    {
        /// <summary>
        /// Gets or sets the credentials for Binance
        /// </summary>
        public BinanceCredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the receive window for Binance
        /// </summary>
        public long ReceiveWindow { get; set; }
    }
}