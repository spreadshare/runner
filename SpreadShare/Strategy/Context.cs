using System.Collections.Generic;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Context is a dictionary in which states can put values
    /// </summary>
    internal class Context
    {
        private readonly Dictionary<string, object> _dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        public Context()
        {
            _dict = new Dictionary<string, object>();
        }

        /// <summary>
        /// Puts object in the dictionary
        /// </summary>
        /// <param name="key">The object key</param>
        /// <param name="value">The object</param>
        public void PutObject(string key, object value)
        {
            if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, value);
            }
            else
            {
                _dict[key] = value;
            }
        }

        /// <summary>
        /// Gets the object by key
        /// </summary>
        /// <param name="key">Key of the object</param>
        /// <returns>The object belonging to the key</returns>
        public object GetObject(string key) => _dict[key];
    }
}
