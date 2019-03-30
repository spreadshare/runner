using SpreadShare.Models.Trading;

namespace SpreadShare.Models
{
    /// <summary>
    /// ResponseObject with string data.
    /// </summary>
    internal class ResponseObject : ResponseObject<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseObject"/> class.
        /// </summary>
        /// <param name="code">Status of the response.</param>
        /// <param name="data">Data concerning the response.</param>
        public ResponseObject(ResponseCode code, string data = "")
            : base(code, data, data)
        {
        }

        /// <summary>
        /// Gets a response object indicating a refused order.
        /// </summary>
        public static ResponseObject<OrderUpdate> OrderRefused =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order was refused by allocation manager");

        /// <summary>
        /// Gets a response object indicating an out of funds exception.
        /// </summary>
        public static ResponseObject<OrderUpdate> OutOfFunds =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, "Portfolio is out of funds");

        /// <inheritdoc/>
        public override string Data => Message;

        /// <summary>
        /// Gets a response object indicating a failed order.
        /// </summary>
        /// <param name="reason">The reason for the failure.</param>
        /// <returns>Desired response object.</returns>
        public static ResponseObject<OrderUpdate> OrderPlacementFailed(string reason) =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, $"Error while placing order: {reason}");

        /// <inheritdoc />
        public override string ToString() => $"{Code} | msg: {Message}";
    }
}