namespace SpreadShare.SupportServices.SettingsService
{
    /// <summary>
    /// Object containing the credentials for an exchange
    /// </summary>
    public class BinanceCredentials
    {
        /// <summary>
        /// Gets the API key
        /// </summary>
        public string Key { get; set;}

        /// <summary>
        /// Gets the API secret
        /// </summary>
        public string Secret { get; set; }
    }
}