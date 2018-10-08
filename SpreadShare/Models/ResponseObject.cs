namespace SpreadShare.Models
{
    /// <summary>
    /// ResponseObject with string data
    /// </summary>
    internal class ResponseObject : ResponseObject<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject"/> class.
        /// </summary>
        /// <param name="code">Status of the response</param>
        /// <param name="data">Data concerning the response</param>
        public ResponseObject(ResponseCodes code, string data = "")
            : base(code, data)
        {
        }

        /// <inheritdoc />
        public override string ToString() => $"{Code} | msg: {Message}";
    }
}