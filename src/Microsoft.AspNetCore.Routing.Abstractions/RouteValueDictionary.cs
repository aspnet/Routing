// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Routing.Abstractions;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// An <see cref="IDictionary{String, Object}"/> type for route values.
    /// </summary>
    public class RouteValueDictionary : IDictionary<string, object>, IReadOnlyDictionary<string, object>
    {
        // 4 is a good default capacity here because that leaves enough space for area/controller/action/id
        private const int DefaultCapacity = 4;

        internal KeyValuePair<string, object>[] _arrayStorage;
        internal PropertyStorage _propertyStorage;
        private int _count;

        /// <summary>
        /// Creates a new instance of <see cref="RouteValueDictionary"/> from the provided array.
        /// The new instance will take ownership of the array, and may mutate it.
        /// </summary>
        /// <param name="items">The items array.</param>
        /// <returns>A new <see cref="RouteValueDictionary"/>.</returns>
        public static RouteValueDictionary FromArray(KeyValuePair<string, object>[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            // We need to compress the array by removing non-contiguous items. We
            // typically have a very small number of items to process. We don't need
            // to preserve order.
            var start = 0;
            var end = items.Length - 1;

            // We walk forwards from the beginning of the array and fill in 'null' slots.
            // We walk backwards from the end of the array end move items in non-null' slots
            // into whatever start is pointing to. O(n)
            while (start <= end)
            {
                if (items[start].Key != null)
                {
                    start++;
                }
                else if (items[end].Key != null)
                {
                    // Swap this item into start and advance
                    items[start] = items[end];
                    items[end] = default;
                    start++;
                    end--;
                }
                else
                {
                    // Both null, we need to hold on 'start' since we
                    // still need to fill it with something.
                    end--;
                }
            }

            return new RouteValueDictionary()
            {
                _arrayStorage = items,
                _count = start,
            };
        }

        /// <summary>
        /// Creates an empty <see cref="RouteValueDictionary"/>.
        /// </summary>
        public RouteValueDictionary()
        {
            _arrayStorage = Array.Empty<KeyValuePair<string, object>>();
        }

        /// <summary>
        /// Creates a <see cref="RouteValueDictionary"/> initialized with the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">An object to initialize the dictionary. The value can be of type
        /// <see cref="IDictionary{TKey, TValue}"/> or <see cref="IReadOnlyDictionary{TKey, TValue}"/>
        /// or an object with public properties as key-value pairs.
        /// </param>
        /// <remarks>
        /// If the value is a dictionary or other <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{String, Object}"/>,
        /// then its entries are copied. Otherwise the object is interpreted as a set of key-value pairs where the
        /// property names are keys, and property values are the values, and copied into the dictionary.
        /// Only public instance non-index properties are considered.
        /// </remarks>
        public RouteValueDictionary(object values)
            : this()
        {
            if (values is RouteValueDictionary dictionary)
            {
                if (dictionary._propertyStorage != null)
                {
                    // PropertyStorage is immutable so we can just copy it.
                    _propertyStorage = dictionary._propertyStorage;
                    _count = dictionary._count;
                    return;
                }

                var other = dictionary._arrayStorage;
                var storage = new KeyValuePair<string, object>[other.Length];
                if (dictionary._count != 0)
                {
                    Array.Copy(other, 0, storage, 0, dictionary._count);
                }

                _arrayStorage = storage;
                _count = dictionary._count;
                return;
            }

            if (values is IEnumerable<KeyValuePair<string, object>> keyValueEnumerable)
            {
                foreach (var kvp in keyValueEnumerable)
                {
                    Add(kvp.Key, kvp.Value);
                }

                return;
            }

            if (values is IEnumerable<KeyValuePair<string, string>> stringValueEnumerable)
            {
                foreach (var kvp in stringValueEnumerable)
                {
                    Add(kvp.Key, kvp.Value);
                }

                return;
            }

            if (values != null)
            {
                var storage = new PropertyStorage(values);
                _propertyStorage = storage;
                _count = storage.Properties.Length;
                return;
            }
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    ThrowArgumentNullExceptionForKey();
                }

                object value;
                TryGetValue(key, out value);
                return value;
            }

            set
            {
                if (key == null)
                {
                    ThrowArgumentNullExceptionForKey();
                }

                // We're calling this here for the side-effect of converting from properties
                // to array. We need to create the array even if we just set an existing value since
                // property storage is immutable. 
                EnsureCapacity(_count);

                var index = FindIndex(key);
                if (index < 0)
                {
                    EnsureCapacity(_count + 1);
                    _arrayStorage[_count++] = new KeyValuePair<string, object>(key, value);
                }
                else
                {
                    _arrayStorage[index] = new KeyValuePair<string, object>(key, value);
                }
            }
        }

        /// <summary>
        /// Gets the comparer for this dictionary.
        /// </summary>
        /// <remarks>
        /// This will always be a reference to <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </remarks>
        public IEqualityComparer<string> Comparer => StringComparer.OrdinalIgnoreCase;

        /// <inheritdoc />
        public int Count => _count;

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                EnsureCapacity(_count);

                var array = _arrayStorage;
                var keys = new string[_count];
                for (var i = 0; i < keys.Length; i++)
                {
                    keys[i] = array[i].Key;
                }

                return keys;
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        /// <inheritdoc />
        public ICollection<object> Values
        {
            get
            {
                EnsureCapacity(_count);

                var array = _arrayStorage;
                var values = new object[_count];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = array[i].Value;
                }

                return values;
            }
        }

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            EnsureCapacity(_count + 1);

            var index = FindIndex(key);
            if (index >= 0)
            {
                var message = Resources.FormatRouteValueDictionary_DuplicateKey(key, nameof(RouteValueDictionary));
                throw new ArgumentException(message, nameof(key));
            }

            _arrayStorage[_count] = new KeyValuePair<string, object>(key, value);
            _count++;
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (_count == 0)
            {
                return;
            }

            if (_propertyStorage != null)
            {
                _arrayStorage = Array.Empty<KeyValuePair<string, object>>();
                _propertyStorage = null;
                _count = 0;
                return;
            }

            Array.Clear(_arrayStorage, 0, _count);
            _count = 0;
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return TryGetValue(item.Key, out var value) && EqualityComparer<object>.Default.Equals(value, item.Value);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            return TryGetValue(key, out var _);
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.CopyTo(
            KeyValuePair<string, object>[] array,
            int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex > array.Length || array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (Count == 0)
            {
                return;
            }

            EnsureCapacity(Count);

            var storage = _arrayStorage;
            Array.Copy(storage, 0, array, arrayIndex, _count);
        }

        /// <inheritdoc />
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            if (Count == 0)
            {
                return false;
            }

            EnsureCapacity(Count);

            var index = FindIndex(item.Key);
            var array = _arrayStorage;
            if (index >= 0 && EqualityComparer<object>.Default.Equals(array[index].Value, item.Value))
            {
                Array.Copy(array, index + 1, array, index, _count - index);
                _count--;
                array[_count] = default;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            if (Count == 0)
            {
                return false;
            }

            EnsureCapacity(Count);

            var index = FindIndex(key);
            if (index >= 0)
            {
                _count--;
                var array = _arrayStorage;
                Array.Copy(array, index + 1, array, index, _count - index);
                array[_count] = default;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="RouteValueDictionary"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the <see cref="RouteValueDictionary"/>, or <c>null</c> if key does not exist.</param>
        /// <returns>
        /// <c>true</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(string key, out object value)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            if (Count == 0)
            {
                value = default;
                return false;
            }

            EnsureCapacity(Count);

            var index = FindIndex(key);
            if (index >= 0)
            {
                _count--;
                var array = _arrayStorage;
                value = array[index].Value;
                Array.Copy(array, index + 1, array, index, _count - index);
                array[_count] = default;

                return true;
            }

            value = default;
            return false;
        }


        /// <summary>
        /// Attempts to the add the provided <paramref name="key"/> and <paramref name="value"/> to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if the value was added. Returns <c>false</c> if the key was already present.</returns>
        public bool TryAdd(string key, object value)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            // Since this is an attempt to write to the dictionary, just make it an array if it isn't. If the code
            // path we're on event tries to write to the dictionary, it will likely get 'upgraded' at some point,
            // so we do it here to keep the code size and complexity down.
            EnsureCapacity(Count);

            var index = FindIndex(key);
            if (index >= 0)
            {
                return false;
            }

            EnsureCapacity(Count + 1);
            _arrayStorage[Count] = new KeyValuePair<string, object>(key, value);
            _count++;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                ThrowArgumentNullExceptionForKey();
            }

            if (_propertyStorage == null)
            {
                return TryFindItem(key, out value);
            }

            return TryGetValueSlow(key, out value);
        }

        private bool TryGetValueSlow(string key, out object value)
        {
            if (_propertyStorage != null)
            {
                var storage = _propertyStorage;
                for (var i = 0; i < storage.Properties.Length; i++)
                {
                    if (string.Equals(storage.Properties[i].Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = storage.Properties[i].GetValue(storage.Value);
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static void ThrowArgumentNullExceptionForKey()
        {
            throw new ArgumentNullException("key");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int capacity)
        {
            if (_propertyStorage != null || _arrayStorage.Length < capacity)
            {
                EnsureCapacitySlow(capacity);
            }
        }

        private void EnsureCapacitySlow(int capacity)
        {
            if (_propertyStorage != null)
            {
                var storage = _propertyStorage;
                
                // If we're converting from properties, it's likely due to an 'add' to make sure we have at least
                // the default amount of space.
                capacity = Math.Max(DefaultCapacity, Math.Max(storage.Properties.Length, capacity));
                var array = new KeyValuePair<string, object>[capacity];

                for (var i = 0; i < storage.Properties.Length; i++)
                {
                    var property = storage.Properties[i];
                    array[i] = new KeyValuePair<string, object>(property.Name, property.GetValue(storage.Value));
                }

                _arrayStorage = array;
                _propertyStorage = null;
                return;
            }

            if (_arrayStorage.Length < capacity)
            {
                capacity = _arrayStorage.Length == 0 ? DefaultCapacity : _arrayStorage.Length * 2;
                var array = new KeyValuePair<string, object>[capacity];
                if (_count > 0)
                {
                    Array.Copy(_arrayStorage, 0, array, 0, _count);
                }

                _arrayStorage = array;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindIndex(string key)
        {
            // Generally the bounds checking here will be elided by the JIT because this will be called
            // on the same code path as EnsureCapacity.
            var array = _arrayStorage;
            var count = _count;

            for (var i = 0; i < count; i++)
            {
                if (string.Equals(array[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryFindItem(string key, out object value)
        {
            var array = _arrayStorage;
            var count = _count;

            // Elide bounds check for indexing.
            if ((uint)count <= (uint)array.Length)
            {
                for (var i = 0; i < count; i++)
                {
                    if (string.Equals(array[i].Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        value = array[i].Value;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private readonly RouteValueDictionary _dictionary;
            private int _index;

            public Enumerator(RouteValueDictionary dictionary)
            {
                if (dictionary == null)
                {
                    throw new ArgumentNullException();
                }

                _dictionary = dictionary;

                Current = default;
                _index = 0;
            }

            public KeyValuePair<string, object> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            // Similar to the design of List<T>.Enumerator - Split into fast path and slow path for inlining friendliness
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var dictionary = _dictionary;

                // The uncommon case is that the propertyStorage is in use
                if (dictionary._propertyStorage == null && ((uint)_index < (uint)dictionary._count))
                {
                    Current = dictionary._arrayStorage[_index];
                    _index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                var dictionary = _dictionary; 
                if (dictionary._propertyStorage != null && ((uint)_index < (uint)dictionary._count))
                {
                    var storage = dictionary._propertyStorage;
                    var property = storage.Properties[_index];
                    Current = new KeyValuePair<string, object>(property.Name, property.GetValue(storage.Value));
                    _index++;
                    return true;
                }

                _index = dictionary._count;
                Current = default;
                return false;
            }

            public void Reset()
            {
                Current = default;
                _index = 0;
            }
        }

        internal class PropertyStorage
        {
            private static readonly ConcurrentDictionary<Type, PropertyHelper[]> _propertyCache = new ConcurrentDictionary<Type, PropertyHelper[]>();

            public readonly object Value;
            public readonly PropertyHelper[] Properties;

            public PropertyStorage(object value)
            {
                Debug.Assert(value != null);
                Value = value;

                // Cache the properties so we can know if we've already validated them for duplicates.
                var type = Value.GetType();
                if (!_propertyCache.TryGetValue(type, out Properties))
                {
                    Properties = PropertyHelper.GetVisibleProperties(type);
                    ValidatePropertyNames(type, Properties);
                    _propertyCache.TryAdd(type, Properties);
                }
            }

            private static void ValidatePropertyNames(Type type, PropertyHelper[] properties)
            {
                var names = new Dictionary<string, PropertyHelper>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];

                    if (names.TryGetValue(property.Name, out var duplicate))
                    {
                        var message = Resources.FormatRouteValueDictionary_DuplicatePropertyName(
                            type.FullName,
                            property.Name,
                            duplicate.Name,
                            nameof(RouteValueDictionary));
                        throw new InvalidOperationException(message);
                    }

                    names.Add(property.Name, property);
                }
            }
        }
    }
}