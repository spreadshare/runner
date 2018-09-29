using System.Collections.Generic;

namespace SpreadShare.Strategy
{
    /// <summary>
    /// Context is a dictionary in which states can put values
    /// </summary>
    internal class Context
    {
        private readonly Dictionary<string, object> _dict;

        public Context()
        {
            _dict = new Dictionary<string, object>();
        }

        public void SetObject(string key, object value)
        {
            if (!_dict.ContainsKey(key))
                _dict.Add(key, value);
            else
                _dict[key] = value;
        }

        public object GetObject(string key)
        {
            return _dict[key];
        }
    }
}
