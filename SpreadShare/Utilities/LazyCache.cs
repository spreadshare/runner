using System;

namespace SpreadShare.Utilities
{
    /// <summary>
    /// Builds a given item once and then keeps the cached value.
    /// </summary>
    /// <typeparam name="TI">The input type of the transformation.</typeparam>
    /// <typeparam name="TO">The output type of the transformation.</typeparam>
    public class LazyCache<TI, TO>
    {
        private readonly Func<TI, TO> _transform;
        private TO _cached;
        private bool _build;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCache{TI,TO}"/> class.
        /// </summary>
        /// <param name="transform">The transformation method.</param>
        public LazyCache(Func<TI, TO> transform)
        {
            _transform = transform;
        }

        /// <summary>
        /// Return the value of an item by evaluating the transformation function once.
        /// </summary>
        /// <param name="data">The data to transform.</param>
        /// <returns>The transformed data.</returns>
        public TO Value(TI data)
        {
            if (!_build)
            {
                _cached = _transform(data);
                _build = true;
            }

            return _cached;
        }
    }
}