using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Common.Classes
{
    internal class LockedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                lock (Dictionary)
                {
                    return Dictionary[key];
                }
            }

            set
            {
                lock (Dictionary)
                {
                    Dictionary[key] = value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (Dictionary)
                {
                    return Dictionary.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (Dictionary)
                {
                    return Dictionary.Values;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (Dictionary)
                {
                    return Dictionary.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
        lock (Dictionary)
        {
            Dictionary.Add(key, value);
        }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (Dictionary)
            {
                (Dictionary as IDictionary<TKey, TValue>).Add(item);
            }
        }

        public void Clear()
        {
            lock (Dictionary)
            {
                Dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (Dictionary)
            {
                return Dictionary.Contains(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (Dictionary)
            {
                return Dictionary.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (Dictionary)
            {
                (Dictionary as IDictionary<TKey, TValue>).CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (Dictionary)
            {
                return Dictionary.GetEnumerator();
            }
        }

        public bool Remove(TKey key)
        {
            lock (Dictionary)
            {
                return Dictionary.Remove(key);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (Dictionary)
            {
                return (Dictionary as IDictionary<TKey, TValue>).Remove(item);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (Dictionary)
            {
                return Dictionary.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (Dictionary)
            {
                return Dictionary.GetEnumerator();
            }
        }
    }
}
