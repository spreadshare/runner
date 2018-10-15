namespace SpreadShare.Models
{
    /// <summary>
    /// Object representing the response of a performed action
    /// </summary>
    /// <typeparam name="T">Type of the response data</typeparam>
    internal class ResponseObject<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="codes">Status of the response</param>
        /// <param name="msg">Message concerning the status</param>
        public ResponseObject(ResponseCodes codes, string msg)
        {
            Codes = codes;
            Message = msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="codes">Status of the response</param>
        /// <param name="data">Data concerning the response</param>
        /// <param name="message">Message concerning the status</param>
        public ResponseObject(ResponseCodes codes, T data, string message = "")
        {
            Codes = codes;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="codes">Status of the response</param>
        public ResponseObject(ResponseCodes codes)
        {
            Codes = codes;
            Message = string.Empty;
        }

        /// <summary>
        /// Gets the code of the response
        /// </summary>
        public ResponseCodes Codes { get; }

        /// <summary>
        /// Gets the message of the response
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the data of the response
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets a value indicating whether returns whether the response was a success
        /// </summary>
        public bool Success => Codes == ResponseCodes.Success;

        /// <inheritdoc />
        public override string ToString() => $"{Codes} | data: {Data} | msg: {Message}";
    }
}
