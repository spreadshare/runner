namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Settings for the Binance exchange
    /// </summary>
    public class BinanceSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceSettings"/> class.
        /// </summary>
        /// <param name="authy">Authenticator object for Binance</param>
        /// <param name="receiveWindow">Window</param>
        public BinanceSettings(Authy authy, long receiveWindow)
        {
            Credentials = authy;
            ReceiveWindow = receiveWindow;
        }

        /// <summary>
        /// Gets the credentials for Binance
        /// </summary>
        public Authy Credentials { get; }

        /// <summary>
        /// Gets the receive window for Binance
        /// TODO: What is this?
        /// </summary>
        public long ReceiveWindow { get; }
    }
}