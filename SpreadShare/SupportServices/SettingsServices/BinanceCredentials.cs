namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Object containing the credentials for an exchange
    /// </summary>
    public class BinanceCredentials
    {
        /// <summary>
        /// Gets or sets the API key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the API secret
        /// </summary>
        public string Secret { get; set; }
    }
}