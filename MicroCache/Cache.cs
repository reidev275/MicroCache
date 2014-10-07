using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroCache
{
    /// <summary>
    /// Provides a generic caching mechanism for any ICloneable object.
    /// </summary>
    /// <typeparam name="TKey">Underlying Dictionary Key</typeparam>
    /// <typeparam name="T">Type of object to cache</typeparam>
    internal class Cache<TKey, T> where T : ICloneable
    {
        readonly TimeSpan _cacheDuration;
        readonly int _maxRecordsToCache = 100;
        protected static readonly object _locker = new object();
        protected static readonly IDictionary<TKey, ICached<T>> Dictionary = new Dictionary<TKey, ICached<T>>();

        /// <summary>
        /// Instantiates a new Cache object with a specific duration and maximum records
        /// </summary>
        /// <param name="cacheDuration">Length of time for a cached object to be marked for refresh.</param>
        /// <param name="maxRecordsToCache">Maximum records that this cache can contain.</param>
        public Cache(TimeSpan cacheDuration, int maxRecordsToCache)
        {
            if (maxRecordsToCache > 0) _maxRecordsToCache = maxRecordsToCache;
            _cacheDuration = cacheDuration;
        }

        /// <summary>
        /// Instantiates a new Cache object with a 1 minute duration and 100 maximum records
        /// </summary>
        public Cache() : this(TimeSpan.FromMinutes(1), 100) { }

        public virtual T GetValue<TArg>(TKey key, Func<TArg, T> method, TArg argument)
        {
            ICached<T> result = GetCachedResult(key);

            if (result == null)
            {
                result = FindObject(method, argument);
                UpdateCache(key, result);
            }
            return (T)result.Object.Clone();
        }

        /// <summary>
        /// Provides a way to explicitly force a refresh for the given key
        /// </summary>
        /// <param name="key"></param>
        public void MakeStale(TKey key)
        {
            lock (_locker)
            {
                if (Dictionary.ContainsKey(key))
                {
                    Dictionary.Remove(key);
                }
            }
        }

        ICached<T> GetCachedResult(TKey key)
        {
            ICached<T> result;
            lock (_locker)
            {
                if (Dictionary.TryGetValue(key, out result))
                {
                    if (result.LastAccessed < (DateTime.Now - _cacheDuration))
                    {
                        Dictionary.Remove(key);
                        result = null;
                    }
                }
            }
            return result;
        }

        ICached<T> FindObject<TArg>(Func<TArg, T> method, TArg argument)
        {
            T foundObject = method.Invoke(argument);
            return new CachedBase<T>(foundObject);
        }

        void UpdateCache(TKey key, ICached<T> value)
        {
            lock (_locker)
            {
                if (Dictionary.Count >= _maxRecordsToCache)
                {
                    var oldest = Dictionary.OrderBy(x => x.Value.LastAccessed).Select(x => x.Key).First();
                    Dictionary.Remove(oldest);
                }
                Dictionary.Add(key, value);
            }
        }
    }
}
