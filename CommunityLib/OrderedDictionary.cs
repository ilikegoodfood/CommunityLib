using System;
using System.Collections;
using System.Collections.Generic;

namespace CommunityLib
{
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly List<TKey> keys = new List<TKey>();
        private readonly List<TValue> values = new List<TValue>();
        private readonly Dictionary<TKey, int> keyIndexMap = new Dictionary<TKey, int>();

        // Get or set a value by index
        public TValue this[int index]
        {
            get => values[index];
            set
            {
                values[index] = value;
            }
        }

        // Get or set a value by key
        public TValue this[TKey key]
        {
            get
            {
                if (keyIndexMap.TryGetValue(key, out int index))
                {
                    return values[index];
                }
                throw new KeyNotFoundException($"Key '{key}' not found.");
            }
            set
            {
                if (keyIndexMap.TryGetValue(key, out int index))
                {
                    values[index] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public TKey GetKeyAtIndex(int index)
        {
            if (index < 0 || index >= keys.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            return keys[index];
        }

        public void Add(KeyValuePair<TKey, TValue> kvp)
        {
            Add(kvp.Key, kvp.Value);
        }

        // Add a new key-value pair
        public void Add(TKey key, TValue value)
        {
            if (keyIndexMap.ContainsKey(key))
            {
                throw new ArgumentException($"An item with the key '{key}' already exists.");
            }

            keyIndexMap[key] = keys.Count;
            keys.Add(key);
            values.Add(value);
        }

        // Check if the collection contains a key
        public bool ContainsKey(TKey key)
        {
            return keyIndexMap.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return values.Contains(value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return keyIndexMap.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(item.Value, values[index]);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (keyIndexMap.TryGetValue(key, out int index))
            {
                value = values[index];
                return true;
            }
            value = default;
            return false;
        }

        // Get the index of a key
        public int IndexOf(TKey key)
        {
            if (keyIndexMap.TryGetValue(key, out int index))
            {
                return index;
            }
            return -1;  // Key not found
        }

        public int IndexOf(TValue value)
        {
            return values.IndexOf(value);
        }

        // Remove an item by key
        public bool Remove(TKey key)
        {
            if (keyIndexMap.TryGetValue(key, out int index))
            {
                // Remove from lists and update indexes in the map
                keys.RemoveAt(index);
                values.RemoveAt(index);
                keyIndexMap.Remove(key);
                UpdateIndexes(index);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (keyIndexMap.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(item.Value, values[index]))
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);
                keyIndexMap.Remove(item.Key);
                UpdateIndexes(index);
                return true;
            }

            return false;
        }

        public bool Remove(TValue value)
        {
            int index = IndexOf(value);
            if (index == -1)
            {
                return false;
            }

            var key = keys[index];
            keys.RemoveAt(index);
            values.RemoveAt(index);
            keyIndexMap.Remove(key);
            UpdateIndexes(index);
            return true;
        }

        // Remove an item by index
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= keys.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var key = keys[index];
            keys.RemoveAt(index);
            values.RemoveAt(index);
            keyIndexMap.Remove(key);
            UpdateIndexes(index);
        }

        // Number of items in the collection
        public int Count => keys.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            keys.Clear();
            values.Clear();
            keyIndexMap.Clear();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException("The target array is not large enough to hold the elements.");
            }

            for (int i = 0; i < keys.Count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        // Update the key-index map after removal
        private void UpdateIndexes(int startIndex)
        {
            for (int i = startIndex; i < keys.Count; i++)
            {
                keyIndexMap[keys[i]] = i;
            }
        }

        // Get the keys as a collection
        public ICollection<TKey> Keys => keys.AsReadOnly();
        public ICollection<TValue> Values => values.AsReadOnly();

        // Enumerator for looping
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
