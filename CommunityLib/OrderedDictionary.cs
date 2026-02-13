using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        private int _version = 0;
        private readonly List<KeyValuePair<TKey, TValue>> _entries;
        private readonly Dictionary<TKey, int> _keyIndexMap;

        private readonly KeyCollection _keysView;
        private readonly ValueCollection _valuesView;

        // Get or set a value by index
        public TValue this[int index]
        {
            get => _entries[index].Value;
            set
            {
                _entries[index] = new KeyValuePair<TKey, TValue>(_entries[index].Key, value);
                _version++;
            }
        }

        // Get or set a value by key
        public TValue this[TKey key]
        {
            get
            {
                if (_keyIndexMap.TryGetValue(key, out int index))
                {
                    return _entries[index].Value;
                }
                throw new KeyNotFoundException($"Key '{key}' not found.");
            }
            set
            {
                if (_keyIndexMap.TryGetValue(key, out int index))
                {
                    _entries[index] = new KeyValuePair<TKey, TValue>(key, value);
                    _version++;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        private TKey GetKeyAtIndex(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            return _entries[index].Key;
        }

        private KeyValuePair<TKey, TValue> GetKeyValuePairAtIndex(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            return _entries[index];
        }

        //prvate master constructor using dummy bool for unique signature.
        private OrderedDictionary(bool isInternal, int? capacity, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            if (collection == null)
            {
                if (capacity == null)
                {
                    _entries = new List<KeyValuePair<TKey, TValue>>();
                    _keyIndexMap = comparer == null ? new Dictionary<TKey, int>() : new Dictionary<TKey, int>(comparer);
                }
                else if (capacity.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(capacity), "Must be a positive value.");
                }
                else
                {
                    _entries = new List<KeyValuePair<TKey, TValue>>(capacity.Value);
                    _keyIndexMap = comparer == null ? new Dictionary<TKey, int>(capacity.Value) : new Dictionary<TKey, int>(capacity.Value, comparer);
                }
            }
            else
            {
                List<KeyValuePair<TKey, TValue>> items = collection as List<KeyValuePair<TKey, TValue>> ?? new List<KeyValuePair<TKey, TValue>>(collection);
                HashSet<TKey> keySet = comparer == null ? new HashSet<TKey>() : new HashSet<TKey>(comparer);
                foreach (KeyValuePair<TKey, TValue> kvp in items)
                {
                    if (!keySet.Add(kvp.Key))
                    {
                        throw new ArgumentException($"The collection to be added contained a duplicate key {kvp.Key}.");
                    }
                }

                int finalCapacity = capacity == null ? items.Count : Math.Max(items.Count, capacity.Value);
                _entries = new List<KeyValuePair<TKey, TValue>>(finalCapacity);
                _keyIndexMap = comparer == null ? new Dictionary<TKey, int>(finalCapacity) : new Dictionary<TKey, int>(finalCapacity, comparer);

                for (int i = 0; i < items.Count; i++)
                {
                    KeyValuePair<TKey, TValue> kvp = items[i];

                    _entries.Add(kvp);
                    _keyIndexMap.Add(kvp.Key, i);
                }
            }

            _keysView = new KeyCollection(this);
            _valuesView = new ValueCollection(this);
        }

        public OrderedDictionary() : this(true, null, null, null) { }

        public OrderedDictionary(int capacity) : this(true, capacity, null, null) { }

        public OrderedDictionary(int capacity, IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(true, capacity, collection, null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
        }

        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer) : this(true, capacity, null, comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
        }

        public OrderedDictionary(int capacity, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(true, capacity, collection, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
        }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(true, null, collection, null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
        }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(true, null, collection, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer) : this(true, null, null, comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
        }

        // Add a new key-value pair
        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Add(KeyValuePair<TKey, TValue> kvp)
        {
            if (_keyIndexMap.ContainsKey(kvp.Key))
            {
                throw new ArgumentException($"An item with the key '{kvp.Key}' already exists.", nameof(kvp));
            }

            _keyIndexMap[kvp.Key] = _entries.Count;
            _entries.Add(kvp);
            _version++;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return TryAdd(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool TryAdd(KeyValuePair<TKey, TValue> kvp)
        {
            if (_keyIndexMap.ContainsKey(kvp.Key))
            {
                return false;
            }

            _keyIndexMap[kvp.Key] = Count;
            _entries.Add(kvp);
            _version++;
            return true;
        }

        // Check if the collection contains a key
        public bool ContainsKey(TKey key)
        {
            return _keyIndexMap.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _keyIndexMap.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(item.Value, _entries[index].Value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyIndexMap.TryGetValue(key, out int index))
            {
                value = _entries[index].Value;
                return true;
            }
            value = default;
            return false;
        }

        // Get the index of a key
        public int IndexOf(TKey key)
        {
            if (_keyIndexMap.TryGetValue(key, out int index))
            {
                return index;
            }
            return -1;  // Key not found
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            if (_keyIndexMap.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(item.Value, _entries[index].Value))
            {
                return index;
            }
            return -1;
        }

        public void Insert(int index, TKey key, TValue value)
        {
            Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            if (_keyIndexMap.ContainsKey(item.Key))
            {
                throw new ArgumentException($"An element with Key = {item.Key} already exists.", nameof(item));
            }

            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            _entries.Insert(index, item);
            _keyIndexMap.Add(item.Key, index);
            _version++;

            UpdateIndexes(index);
        }

        public void InsertRange(int index, IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            List<KeyValuePair<TKey, TValue>> items = collection as List<KeyValuePair<TKey, TValue>> ?? new List<KeyValuePair<TKey, TValue>>(collection);
            if (items.Count == 0)
            {
                return;
            }

            HashSet<TKey> keySet = new HashSet<TKey>(_keyIndexMap.Comparer);
            foreach (KeyValuePair<TKey, TValue> kvp in items)
            {
                if (_keyIndexMap.ContainsKey(kvp.Key))
                {
                    throw new ArgumentException($"An item with the key '{kvp.Key}' already exists.");
                }

                if (!keySet.Add(kvp.Key))
                {
                    throw new ArgumentException($"The collection to be added contained a duplicate key {kvp.Key}.");
                }
            }

            foreach (KeyValuePair<TKey, TValue> kvp in items)
            {
                _keyIndexMap.Add(kvp.Key, -1);
            }

            _entries.InsertRange(index, items);

            _version++;
            UpdateIndexes(index);
        }

        // Remove an item by key
        public bool Remove(TKey key)
        {
            if (_keyIndexMap.TryGetValue(key, out int index))
            {
                // Remove from lists and update indexes in the map
                _entries.RemoveAt(index);
                _keyIndexMap.Remove(key);
                _version++;

                UpdateIndexes(index);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_keyIndexMap.TryGetValue(item.Key, out int index) && EqualityComparer<TValue>.Default.Equals(item.Value, _entries[index].Value))
            {
                _entries.RemoveAt(index);
                _keyIndexMap.Remove(item.Key);
                _version++;

                UpdateIndexes(index);
                return true;
            }

            return false;
        }

        // Remove an item by index
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            TKey key = _entries[index].Key;
            _entries.RemoveAt(index);
            _keyIndexMap.Remove(key);
            _version++;

            UpdateIndexes(index);
        }

        // Bulk removal operations
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} must be greater than 0.");
            }

            if (Count - index < count)
            {
                throw new ArgumentException("Invalid range specified.");
            }

            if (count == 0)
            {
                return;
            }

            for (int i = index; i < index + count; i++)
            {
                _keyIndexMap.Remove(_entries[i].Key);
            }
            _entries.RemoveRange(index, count);
            _version++;

            if (index < Count)
            {
                UpdateIndexes(index);
            }
        }

        public void RemoveAll(IEnumerable<TKey> keys)
        {
            List<int> indexesToRemove = new List<int>();
            foreach (TKey key in keys)
            {
                if (_keyIndexMap.TryGetValue(key, out int index))
                {
                    indexesToRemove.Add(index);
                }
            }

            if (indexesToRemove.Count == 0)
            {
                return;
            }

            indexesToRemove.Sort((a, b) => b.CompareTo(a));
            int firstIndexAffected = indexesToRemove[indexesToRemove.Count - 1];
            foreach (int index in indexesToRemove)
            {
                _keyIndexMap.Remove(_entries[index].Key);
                _entries.RemoveAt(index);
            }
            _version++;

            if (firstIndexAffected < Count)
            {
                UpdateIndexes(firstIndexAffected);
            }
        }

        public int RemoveAll(Predicate<KeyValuePair<TKey, TValue>> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            List<int> indexesToRemove = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                if (match(_entries[i]))
                {
                    indexesToRemove.Add(i);
                }
            }

            if (indexesToRemove.Count == 0)
            {
                return 0;
            }

            indexesToRemove.Sort((a, b) => b.CompareTo(a));
            int firstIndexAffected = indexesToRemove[indexesToRemove.Count - 1];
            foreach (int index in indexesToRemove)
            {
                _keyIndexMap.Remove(_entries[index].Key);
                _entries.RemoveAt(index);
            }
            _version++;

            if (firstIndexAffected < Count)
            {
                UpdateIndexes(firstIndexAffected);
            }
            return indexesToRemove.Count;
        }

        // Number of items in the collection
        public int Count => _entries.Count;

        public bool IsReadOnly => false;

        public void Clear()
        {
            _entries.Clear();
            _keyIndexMap.Clear();
            _version++;
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
                throw new ArgumentException("The target array is not large enough to hold the elements.", nameof(array));
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                array[arrayIndex + i] = _entries[i];
            }
        }

        // Update the key-index map after removal
        private void UpdateIndexes(int startIndex)
        {
            for (int i = startIndex; i < _entries.Count; i++)
            {
                _keyIndexMap[_entries[i].Key] = i;
            }
        }

        // Get the keys as a collection

        public IReadOnlyList<TKey> Keys => _keysView;

        public IReadOnlyList<TValue> Values => _valuesView;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _keysView;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _valuesView;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _keysView;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _valuesView;

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => GetKeyValuePairAtIndex(index);

        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get
            {
                return GetKeyValuePairAtIndex(index);
            }
            set
            {
                if (index < 0 || index >= _entries.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                if (_keyIndexMap.TryGetValue(value.Key, out int i) && i != index)
                {
                    throw new ArgumentException($"An item with the key '{value.Key}' already exists.", nameof(value));
                }

                _entries[index] = value;
                _version++;
            }
        }

        // Enumerator for looping
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            int version = _version;
            for (int i = 0; i < _entries.Count; i++)
            {
                if (version != _version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration");
                }

                yield return _entries[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Reverse()
        {
            int version = _version;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (version != _version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration");
                }

                yield return _entries[i];
            }
        }

        public sealed class KeyCollection : IReadOnlyList<TKey>, ICollection<TKey>
        {
            private readonly OrderedDictionary<TKey, TValue> _parent;
            internal KeyCollection(OrderedDictionary<TKey, TValue> parent) => _parent = parent;

            public int Count => _parent.Count;
            public bool IsReadOnly => true;

            public TKey this[int index] => _parent.GetKeyAtIndex(index);

            public IEnumerator<TKey> GetEnumerator()
            {
                int ver = _parent._version;
                foreach (var entry in _parent._entries)
                {
                    if (ver != _parent._version)
                    {
                        throw new InvalidOperationException("Modified during enumeration");
                    }
                    yield return entry.Key;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Contains(TKey item) => _parent.ContainsKey(item);
            public void CopyTo(TKey[] array, int arrayIndex)
            {
                for (int i = 0; i < _parent.Count; i++)
                {
                    array[arrayIndex + i] = _parent._entries[i].Key;
                }
            }

            // Explicit interface hides these from the primary API
            void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException("This collection is readonly");
            void ICollection<TKey>.Clear() => throw new NotSupportedException("This collection is readonly");
            bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException("This collection is readonly");
        }

        public sealed class ValueCollection : IReadOnlyList<TValue>, ICollection<TValue>
        {
            private readonly OrderedDictionary<TKey, TValue> _parent;
            internal ValueCollection(OrderedDictionary<TKey, TValue> parent) => _parent = parent;

            public int Count => _parent.Count;
            public bool IsReadOnly => true;

            public TValue this[int index] => _parent._entries[index].Value;

            public IEnumerator<TValue> GetEnumerator()
            {
                int ver = _parent._version;
                foreach (var entry in _parent._entries)
                {
                    if (ver != _parent._version)
                    {
                        throw new InvalidOperationException("Modified during enumeration");
                    }
                    yield return entry.Value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Contains(TValue item) => _parent._entries.Any(e => EqualityComparer<TValue>.Default.Equals(e.Value, item));
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                for (int i = 0; i < _parent.Count; i++)
                {
                    array[arrayIndex + i] = _parent._entries[i].Value;
                }
            }

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException("This collection is readonly");
            void ICollection<TValue>.Clear() => throw new NotSupportedException("This collection is readonly");
            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException("This collection is readonly");
        }
    }
}
