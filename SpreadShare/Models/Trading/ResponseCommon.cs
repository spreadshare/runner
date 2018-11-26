namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Common response objects
    /// </summary>
    internal static class ResponseCommon
    {
        /// <summary>
        /// Gets a response object indicating a refused order
        /// </summary>
        public static ResponseObject<OrderUpdate> OrderRefused =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order was refused");
    }
}