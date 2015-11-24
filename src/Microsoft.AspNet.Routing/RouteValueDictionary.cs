// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNet.Routing
{
    /// <summary>
    /// An <see cref="IDictionary{string, object}"/> type for route values.
    /// </summary>
    public class RouteValueDictionary : IDictionary<string, object>, IReadOnlyDictionary<string, object>
    {
        /// <summary>
        /// An empty, cached instance of <see cref="RouteValueDictionary"/>.
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, object> Empty = new RouteValueDictionary();

        private Dictionary<string, object> _dictionary;
        private readonly PropertyHelper[] _properties;
        private readonly object _value;

        /// <summary>
        /// Creates an empty <see cref="RouteValueDictionary"/>.
        /// </summary>
        public RouteValueDictionary()
        {
        }

        /// <summary>
        /// Creates a <see cref="RouteValueDictionary"/> initialized with the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">An object to initialize the dictionary. The value can be of type
        /// <see cref="IDictionary{TKey, TValue}"/> or <see cref="IReadOnlyDictionary{TKey, TValue}"/>
        /// or an object with public properties as key-value pairs.
        /// </param>
        /// <remarks>
        /// If the value is a dictionary or other <see cref="IEnumerable{KeyValuePair{string, object}}"/>,
        /// then its entries are copied. Otherwise the object is interpreted as a set of key-value pairs where the
        /// property names are keys, and property values are the values, and copied into the dictionary.
        /// Only public instance non-index properties are considered.
        /// </remarks>
        public RouteValueDictionary(object values)
        {
            var otherDictionary = values as RouteValueDictionary;
            if (otherDictionary != null)
            {
                if (otherDictionary._dictionary != null)
                {
                    _dictionary = new Dictionary<string, object>(
                        otherDictionary._dictionary.Count,
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var kvp in otherDictionary._dictionary)
                    {
                        _dictionary[kvp.Key] = kvp.Value;
                    }

                    return;
                }
                else if (otherDictionary._properties != null)
                {
                    _properties = otherDictionary._properties;
                    _value = otherDictionary._value;
                    return;
                }
                else
                {
                    return;
                }
            }

            var keyValuePairCollection = values as IEnumerable<KeyValuePair<string, object>>;
            if (keyValuePairCollection != null)
            {
                _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var kvp in keyValuePairCollection)
                {
                    _dictionary[kvp.Key] = kvp.Value;
                }

                return;
            }

            if (values != null)
            {
                _properties = PropertyHelper.GetVisibleProperties(values);
                _value = values;

                return;
            }
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                object value;
                TryGetValue(key, out value);
                return value;
            }

            set
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                EnsureWritable();
                _dictionary[key] = value;
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
        public int Count
        {
            get
            {
                return _dictionary?.Count ?? _properties?.Length ?? 0;
            }
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        /// <inheritdoc />
        public ICollection<string> Keys
        {
            get
            {
                EnsureWritable();
                return _dictionary.Keys;
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys
        {
            get
            {
                EnsureWritable();
                return _dictionary.Keys;
            }
        }

        /// <inheritdoc />
        public ICollection<object> Values
        {
            get
            {
                EnsureWritable();
                return _dictionary.Values;
            }
        }

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values
        {
            get
            {
                EnsureWritable();
                return _dictionary.Values;
            }
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            EnsureWritable();
            ((ICollection<KeyValuePair<string, object>>)_dictionary).Add(item);
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureWritable();
            _dictionary.Add(key, value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            EnsureWritable();
            _dictionary.Clear();
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            EnsureWritable();
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Contains(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.ContainsKey(key);
            }
            else if (_properties != null)
            {
                for (var i = 0; i < _properties.Length; i++)
                {
                    var property = _properties[i];
                    if (Comparer.Equals(property.Name, key))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            else
            {
                return false;
            }
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

            EnsureWritable();
            ((ICollection<KeyValuePair<string, object>>)_dictionary).CopyTo(array, arrayIndex);
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
            EnsureWritable();
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Remove(item);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureWritable();
            return _dictionary.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.TryGetValue(key, out value);
            }
            else if (_properties != null)
            {
                for (var i = 0; i < _properties.Length; i++)
                {
                    var property = _properties[i];
                    if (Comparer.Equals(property.Name, key))
                    {
                        value = property.ValueGetter(_value);
                        return true;
                    }
                }

                value = null;
                return false;
            }
            else
            {
                value = null;
                return false;
            }
        }

        private void EnsureWritable()
        {
            if (_dictionary == null && _properties == null)
            {
                _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else if (_dictionary == null)
            {
                _dictionary = new Dictionary<string, object>(_properties.Length + 1, StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < _properties.Length; i++)
                {
                    var property = _properties[i];
                    _dictionary.Add(property.Property.Name, property.ValueGetter(_value));
                }
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private readonly RouteValueDictionary _dictionary;

            private int _index;
            private Dictionary<string, object>.Enumerator _enumerator;

            public Enumerator(RouteValueDictionary dictionary)
            {
                if (dictionary == null)
                {
                    throw new InvalidOperationException();
                }

                _dictionary = dictionary;

                Current = default(KeyValuePair<string, object>);
                _index = -1;
                _enumerator = _dictionary._dictionary == null ? 
                    default(Dictionary<string, object>.Enumerator) : 
                    _dictionary._dictionary.GetEnumerator();
            }

            public KeyValuePair<string, object> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_dictionary?._dictionary != null)
                {
                    if (_enumerator.MoveNext())
                    {
                        Current = _enumerator.Current;
                        return true;
                    }
                    else
                    {
                        Current = default(KeyValuePair<string, object>);
                        return false;
                    }
                }
                else if (_dictionary?._properties != null)
                {
                    if (++_index < _dictionary._properties.Length)
                    {
                        var property = _dictionary._properties[_index];
                        var value = property.ValueGetter(_dictionary._value);
                        Current = new KeyValuePair<string, object>(property.Name, value);
                        return true;
                    }
                    else
                    {
                        Current = default(KeyValuePair<string, object>);
                        return false;
                    }
                }
                else
                {
                    Current = default(KeyValuePair<string, object>);
                    return false;
                }
            }

            public void Reset()
            {
                Current = default(KeyValuePair<string, object>);
                _index = -1;
                _enumerator = _dictionary?._dictionary == null ?
                    default(Dictionary<string, object>.Enumerator) :
                    _dictionary._dictionary.GetEnumerator();
            }
        }
    }
}
