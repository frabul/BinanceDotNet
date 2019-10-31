using System;
using System.Collections.Generic;
using BinanceExchange.API.Caching.Interfaces;
using BinanceExchange.API.Utility; 

namespace BinanceExchange.API.Caching
{
    /// <summary>
    /// Generic API Cache Manager for use in Binance API
    /// </summary>
    public class APICacheManager : IAPICacheManager
    {
        private readonly object _lockObject = new object(); 
        private readonly IList<string> _cacheKeysList;

        private TimeSpan _defaultExpiryTimespan = new TimeSpan(0, 30, 0);

        public APICacheManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add an object to the cache
        /// </summary>
        /// <typeparam name="T">Type of object to be added</typeparam>
        /// <param name="obj">The object to add</param>
        /// <param name="key">The key to identify the cache entry</param>
        /// <param name="expiry">When the cache should expire</param>
        public void Add<T>(T obj, string key, TimeSpan expiry = default(TimeSpan)) where T : class
        {
            Guard.AgainstNullOrEmpty(key);
            if (expiry == default(TimeSpan))
            {
                expiry = _defaultExpiryTimespan;
            }

            if (Contains(key)) return;
            lock (_lockObject)
            {
                if (Contains(key.ToLower())) return;
                _cacheKeysList.Add(key.ToLower());
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Retrieve an item from the cache
        /// </summary>
        /// <typeparam name="T">The type of the object being retrieved</typeparam>
        /// <param name="key">The key to identify the cache entry</param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if the cache contains an item
        /// </summary>
        /// <param name="key">The key to identify the cache entry</param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove an entry from the cache
        /// </summary>
        /// <param name="key">The key to identify the cache entry</param>
        public void Remove(string key)
        {
            if (!Contains(key)) return;
            lock (_lockObject)
            {
                if (Contains(key.ToLower()))
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Remove all items from the cache
        /// </summary>
        public void RemoveAll()
        {
            foreach (var key in _cacheKeysList)
            {
                Guard.AgainstNullOrEmpty(key);

                Remove(key);
            }
        }
    }
}
