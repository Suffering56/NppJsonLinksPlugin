using System;
using System.Collections.Generic;
using NppPluginForHC.Logic;

namespace NppPluginForHC.Core
{
    public interface IExtendedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TValue : class
    {
        // TValue this[TKey key] { get; }

        TValue GetOrNull(TKey key);

        TValue GetOrDefault(TKey key, TValue defaultValue);

        /**
         * @return old value (if present) or computed value (if absent)
         */
        TValue ComputeIfAbsent(TKey key, Func<TKey, TValue> valueSupplier);
    }

    public class ExtendedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IExtendedDictionary<TKey, TValue>
        where TValue : class
    {
        // TValue IMap<TKey, TValue>.this[TKey key] => GetOrNull(key);

        public TValue GetOrNull(TKey key)
        {
            return GetOrDefault(key, null);
        }

        public TValue GetOrDefault(TKey key, TValue defaultValue)
        {
            return TryGetValue(key, out TValue value)
                ? value
                : defaultValue;
        }

        public TValue ComputeIfAbsent(TKey key, Func<TKey, TValue> valueSupplier)
        {
            var value = GetOrNull(key);
            if (value != null) return value;

            value = valueSupplier.Invoke(key);
            this[key] = value;

            return value;
        }
    }
}