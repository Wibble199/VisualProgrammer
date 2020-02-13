using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VisualProgrammer.Core.Utils {

    /// <summary>
    /// Data storage class that can be accessed using a key or an index.
    /// </summary>
    /// <typeparam name="TKey">The type that is used as the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type that is used as the dictionary's values.</typeparam>
    public sealed class IndexedDictionary<TKey, TValue> : IList<TValue>, IDictionary<TKey, TValue> where TKey : notnull {

        private List<(TKey key, TValue value)> internalList = new List<(TKey key, TValue value)>();

        private readonly IEqualityComparer<TKey> keyComparer = EqualityComparer<TKey>.Default;
        private readonly IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

		#region Ctors
		public IndexedDictionary() { }

        public IndexedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) {
			internalList = collection.Select(kvp => (kvp.Key, kvp.Value)).ToList();
		}

		public IndexedDictionary(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) {
            this.keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

		public IndexedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer) : this(keyComparer, valueComparer) {
			internalList = collection.Select(kvp => (kvp.Key, kvp.Value)).ToList();
		}
		#endregion

		public TValue this[TKey key] {
            get => internalList.FirstOrDefault(i => keyComparer.Equals(i.key, key)).value;
            set {
                var idx = internalList.FindIndex(i => keyComparer.Equals(i.key, key));
                if (idx > -1)
                    // If key exists, update it
                    internalList[idx] = (key, value);
                else
                    // Otherwise, add it to the end
                    Add(key, value);
            }
        }

        public TValue this[int index] {
            get => internalList[index].value;
            set {
                // As is the behaviour with List<T>s, you cannot append new items using the indexer.
                if (index < 0 || index >= internalList.Count) throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
                var key = internalList[index].key;
                internalList[index] = (key, value);
            }
        }

        public ICollection<TKey> Keys => internalList.Select(x => x.key).ToList();
        public ICollection<TValue> Values => internalList.Select(x => x.value).ToList();

        public int Count => internalList.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value) {
            if (ContainsKey(key)) throw new ArgumentException("An item with the same key has already been added.");
            internalList.Add((key, value));
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Insert(int index, TKey key, TValue value) {
            if (ContainsKey(key)) throw new ArgumentException("An item with the same key has already been added.");
            if (index < 0 || index > internalList.Count) throw new ArgumentOutOfRangeException("Index must be within the bounds of the IndexedDictionary.");
            internalList.Insert(index, (key, value));
        }

        /// <summary>Creates a new <see cref="IndexedDictionary{TKey, TValue}"/> based on this dictionary, passing the keys and values though the provided key selector and value selector.</summary>
        public IndexedDictionary<TOutKey, TOutValue> Map<TOutKey, TOutValue>(Func<TKey, TOutKey> keySelector, Func<TValue, TOutValue> valueSelector) where TOutKey : notnull {
            var d = new IndexedDictionary<TOutKey, TOutValue> {
                internalList = internalList.Select(pair => (keySelector(pair.key), valueSelector(pair.value))).ToList()
            };
            if (d.internalList.GroupBy(pair => pair.key).Any(g => g.Count() > 1)) // Check for duplicate keys
                throw new ArgumentException("An item with the same key has already been added.");
            return d;
        }

        /// <summary>Creates a new <see cref="IndexedDictionary{TKey, TValue}"/> based on this dictionary, passing the values though the provided value selector.</summary>
        public IndexedDictionary<TKey, TOutValue> Map<TOutValue>(Func<TValue, TOutValue> valueSelector) where TOutValue : notnull => new IndexedDictionary<TKey, TOutValue> {
            internalList = internalList.Select(pair => (pair.key, valueSelector(pair.value))).ToList() // Don't need to check for dupe keys since we've not changed them
        };

        /// <summary>Creates a new <see cref="IndexedDictionary{TKey, TValue}"/> based on the given collection, using the specified key selector and value selector.</summary>
        public static IndexedDictionary<TKey, TValue> From<TItem>(IEnumerable<TItem> collection, Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null) {
            var dict = new IndexedDictionary<TKey, TValue>(keyComparer, valueComparer);
            foreach (var item in collection) // We add the items using the "Add" method, rather than setting the internalList directly so that we assert there are no duplicate keys
                dict.Add(keySelector(item), valueSelector(item));
            return dict;
        }

        /// <summary>Creates a new <see cref="IndexedDictionary{TKey, TValue}"/> based on the given collection, using the specified key selector.</summary>
        public static IndexedDictionary<TKey, TValue> From(IEnumerable<TValue> collection, Func<TValue, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null) =>
            From(collection, keySelector, x => x, keyComparer, valueComparer);

        /// <summary>Removes all elements from the <see cref="IndexedDictionary{TKey, TValue}"/>.</summary>
        public void Clear() => internalList.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => internalList.Contains((item.Key, item.Value));
        public bool Contains(TValue item) => internalList.Any(i => valueComparer.Equals(i.value, item));
        public bool ContainsKey(TKey key) => internalList.Any(i => keyComparer.Equals(i.key, key));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => internalList.Select(i => new KeyValuePair<TKey, TValue>(i.key, i.value)).ToArray().CopyTo(array, arrayIndex);
        public void CopyTo(TValue[] array, int arrayIndex) => internalList.Select(i => i.value).ToList().CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => internalList.Select(i => new KeyValuePair<TKey, TValue>(i.key, i.value)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => internalList.Select(i => i.value).GetEnumerator();

        public int IndexOf(TValue item) => internalList.FindIndex(i => valueComparer.Equals(i.value, item));

        public bool Remove(TKey key) => RemoveKey(key);
        public bool Remove(KeyValuePair<TKey, TValue> item) => internalList.Remove((item.Key, item.Value));
        public bool Remove(TKey key, TValue value) => internalList.Remove((key, value));
        public bool Remove(TValue item) => RemoveValue(item);
        public void RemoveAt(int index) => internalList.RemoveAt(index);

        // Incase the TKey and TValue are the same, these methods can help with the ambiguation
        public bool RemoveKey(TKey key) => RemoveWhere((_key, _) => keyComparer.Equals(_key, key));
        public bool RemoveValue(TValue item) => RemoveWhere((_, value) => valueComparer.Equals(value, item));

        /// <summary>Removes the first occurance of the item that matches the given predicate.</summary>
        public bool RemoveWhere(Func<TKey, TValue, bool> predicate) {
            var idx = internalList.FindIndex(item => predicate(item.key, item.value));
            if (idx >= 0) internalList.RemoveAt(idx);
            return idx >= 0;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
            var idx = internalList.FindIndex(i => keyComparer.Equals(i.key, key));
            value = idx >= 0 ? internalList[idx].value : default;
            return idx >= 0;
        }
               
        #region Unimplementable methods
        // These methods cannot be implemented as they don't provide a key and are hidden from the IndexedDictionary API.
        void ICollection<TValue>.Add(TValue item) => throw new NotImplementedException();
        void IList<TValue>.Insert(int index, TValue item) => throw new NotImplementedException();
        #endregion
    }
}
