namespace SpreadShare.Models.Trading
{
    /// <summary>
    /// Return value of websockets
    /// </summary>
    internal class OrderUpdate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderUpdate"/> class.
        /// </summary>
        /// <param name="price">Price of the order update</param>
        public OrderUpdate(decimal price)
        {
            Price = price;
        }

        /// <summary>
        /// Gets the price of the order.
        /// </summary>
        public decimal Price { get; }
    }
}
