namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Object containing the credentials for an exchange
    /// </summary>
    /// TODO: Refactor this into a struct?
    public class Authy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authy"/> class.
        /// </summary>
        /// <param name="key">API key</param>
        /// <param name="secret">API secret</param>
        public Authy(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        /// <summary>
        /// Gets the API key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the API secret
        /// </summary>
        public string Secret { get; }
    }
}