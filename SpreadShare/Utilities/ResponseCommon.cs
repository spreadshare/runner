using SpreadShare.Models;
using SpreadShare.Models.Trading;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Common response objects.
    /// </summary>
    internal static class ResponseCommon
    {
        /// <summary>
        /// Gets a response object indicating a refused order.
        /// </summary>
        public static ResponseObject<OrderUpdate> OrderRefused =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order was refused");

        /// <summary>
        /// Gets a response object indicating a failed order
        /// </summary>
        /// <param name="reason">The reason for the failure</param>
        /// <returns>Desired response object</returns>
        public static ResponseObject<OrderUpdate> OrderPlacementFailed(string reason) =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, $"Error while placing order: {reason}");
    }
}