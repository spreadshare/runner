namespace SpreadShare.Models
{
    /// <summary>
    /// Object representing the response of a performed action.
    /// </summary>
    /// <typeparam name="T">Type of the response data.</typeparam>
    internal class ResponseObject<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="code">Status of the response.</param>
        /// <param name="msg">Message concerning the status.</param>
        public ResponseObject(ResponseCode code, string msg)
        {
            Code = code;
            Message = msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="code">Status of the response.</param>
        /// <param name="data">Data concerning the response.</param>
        /// <param name="message">Message concerning the status.</param>
        public ResponseObject(ResponseCode code, T data, string message = "")
        {
            Code = code;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class.
        /// </summary>
        /// <param name="code">Status of the response.</param>
        public ResponseObject(ResponseCode code)
        {
            Code = code;
            Message = string.Empty;
        }

        /// <summary>
        /// Gets the code of the response.
        /// </summary>
        public ResponseCode Code { get; }

        /// <summary>
        /// Gets the message of the response.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the data of the response.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets a value indicating whether returns whether the response was a success.
        /// </summary>
        public bool Success => Code == ResponseCode.Success;

        /// <inheritdoc />
        public override string ToString() => $"{Code} | data: {Data} | msg: {Message}";
    }
}
