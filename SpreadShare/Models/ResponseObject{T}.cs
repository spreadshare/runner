using Dawn;

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
            Guard.Argument(code).NotEqual(
                ResponseCode.Success,
                x => $"ResponseObject cannot have code {x} but have no data");
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
            if (code == ResponseCode.Success)
            {
                Guard.Argument(data).HasValue();
            }

            Code = code;
            Data = data;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject{T}"/> class with a success code.
        /// </summary>
        /// <param name="data">Data object for the response.</param>
        public ResponseObject(T data)
        {
            Guard.Argument(data).HasValue();
            Code = ResponseCode.Success;
            Data = data;
            Message = string.Empty;
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
        public virtual T Data { get; }

        /// <summary>
        /// Gets a value indicating whether returns whether the response was a success.
        /// </summary>
        public bool Success => Code == ResponseCode.Success;

        /// <inheritdoc />
        public override string ToString() => $"{Code} | data: {Data} | msg: {Message}";
    }
}
