using System;

namespace SpreadShare.Utilities
{
    #pragma warning disable SA1402
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

        /// <summary>
        /// Invalidate the current build, cause a rebuild on the next evaluation.
        /// </summary>
        public void Invalidate()
        {
            _build = false;
        }
    }

    /// <summary>
    /// Builds a given item once and keeps the cached value.
    /// </summary>
    /// <typeparam name="TI">The type of the input.</typeparam>
    public class LazyCache<TI>
    {
        private object _cached;
        private bool _build;

        /// <summary>
        /// Returns the value of an item by evaluation the transform function once.
        /// </summary>
        /// <param name="data">The data to transform.</param>
        /// <param name="transform">mapping from TI to outType.</param>
        /// <param name="outType">The expected output type.</param>
        /// <returns>Transformed object.</returns>
        public object Value(TI data, Func<TI, object> transform, Type outType)
        {
            if (!_build)
            {
                _cached = transform(data);
                _build = true;
            }

            if (!_cached.GetType().IsAssignableFrom(outType))
            {
                throw new InvalidOperationException(
                    $"Transform function gave object of type {_cached} which cannot be converted to {outType}");
            }

            return _cached;
        }

        /// <summary>
        /// Invalidate the current build, cause a rebuild on the next evaluation.
        /// </summary>
        public void Invalidate()
        {
            _build = false;
        }
    }

    #pragma warning restore SA1402
}