using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Treefrog.Utility
{
    public class Mapper<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> _forwardMap = new Dictionary<TKey, TValue>();
        private Dictionary<TValue, TKey> _backwardMap = new Dictionary<TValue, TKey>();

        public int Count
        {
            get { return _forwardMap.Count; }
        }

        public TValue this[TKey key]
        {
            get { return _forwardMap[key]; }
            set
            {
                _forwardMap[key] = value;
                _backwardMap[value] = key;
            }
        }

        public TKey this[TValue val]
        {
            get { return _backwardMap[val]; }
            set
            {
                _forwardMap[value] = val;
                _backwardMap[val] = value;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _forwardMap.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return _forwardMap.Values; }
        }

        public void Add (TKey key, TValue value)
        {
            _forwardMap.Add(key, value);
            _backwardMap.Add(value, key);
        }

        public bool ContainsKey (TKey key)
        {
            return _forwardMap.ContainsKey(key);
        }

        public bool ContainsValue (TValue value)
        {
            return _backwardMap.ContainsKey(value);
        }

        public void Remove (TKey key)
        {
            TValue value;
            if (_forwardMap.TryGetValue(key, out value)) {
                _forwardMap.Remove(key);
                _backwardMap.Remove(value);
            }
        }

        public void Remove (TValue value)
        {
            TKey key;
            if (_backwardMap.TryGetValue(value, out key)) {
                _forwardMap.Remove(key);
                _backwardMap.Remove(value);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
        {
            return _forwardMap.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }
    }
}
