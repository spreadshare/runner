namespace SpreadShare.Models.Trading
{
    internal static class ResponseObjectCommon
    {
        public static ResponseObject<OrderUpdate> ORDER_REFUSED =>
            new ResponseObject<OrderUpdate>(ResponseCode.Error, "Order ");
    }
}