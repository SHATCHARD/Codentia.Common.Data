using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Codentia.Common.Data.Caching
{
    /// <summary>
    /// Class encapsulating access to the current HttpCache. A Cache object will be created if no HttpContext is available.
    /// </summary>
    public static class DataCache
    {
        private static Cache _cache = null;
        private static HttpRuntime _httpRuntime = null;
        private static object _lockObject = new object();
        private static bool _consoleOutput;
        private static List<string> _keyList = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the console output flag. If enabled, this will cause diagnostic output to be written to Console.
        /// </summary>
        public static bool ConsoleOutputEnabled
        {
            get
            {
                bool value;

                lock (_lockObject)
                {
                    value = _consoleOutput;
                }

                return _consoleOutput;
            }

            set
            {
                lock (_lockObject)
                {
                    _consoleOutput = value;
                }
            }
        }

        /// <summary>
        /// Retrieve a value from a cached Dictionary (strongly typed).
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="cacheKey">Identification Key identifying dictionary</param>
        /// <param name="id">Dictionary Key to retrieve</param>
        /// <returns>TValue of Dictionary Value</returns>
        public static TValue GetFromDictionary<TKey, TValue>(string cacheKey, TKey id)
        {
            EnsureCache();

            TValue result = default(TValue);

            if (ContainsKey(cacheKey))
            {
                Dictionary<TKey, TValue> index;

                lock (_lockObject)
                {
                    index = (Dictionary<TKey, TValue>)_cache.Get(cacheKey);
                }

                if (index != null && index.ContainsKey(id))
                {
                    result = index[id];

                    WriteDiagnosticMessage(string.Format("HIT - GetFromDictionary({0}): {1}", cacheKey, id));
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the given cached Dictionary contains the specified key
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="id">The id.</param>
        /// <returns>bool - true </returns>
        public static bool DictionaryContainsKey<TKey, TValue>(string cacheKey, TKey id)
        {
            bool result = false;

            EnsureCache();

            if (ContainsKey(cacheKey))
            {
                Dictionary<TKey, TValue> index;

                lock (_lockObject)
                {
                    index = (Dictionary<TKey, TValue>)_cache.Get(cacheKey);
                }

                if (index != null && index.ContainsKey(id))
                {
                    result = true;

                    WriteDiagnosticMessage(string.Format("HIT - DictionaryContainsKey({0}): {1}", cacheKey, id));
                }
            }

            return result;
        }

        /// <summary>
        /// Add an entry to a cached dictionary. The dictionary will be created if non-existant.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="cacheKey">Key identifying the Dictionary</param>
        /// <param name="id">Dictionary Key to insert/update</param>
        /// <param name="data">Dictionary value to be stored</param>
        public static void AddToDictionary<TKey, TValue>(string cacheKey, TKey id, TValue data)
        {
            EnsureCache();

            lock (_lockObject)
            {
                Dictionary<TKey, TValue> index = (Dictionary<TKey, TValue>)_cache.Get(cacheKey);

                if (index == null)
                {
                    index = new Dictionary<TKey, TValue>();
                    _keyList.Add(cacheKey);
                }

                if (index.ContainsKey(id))
                {
                    index.Remove(id);
                    WriteDiagnosticMessage(string.Format("REMOVE - AddToDictionary({0}): {1}", cacheKey, id));
                }

                index.Add(id, data);
                _cache.Add(cacheKey, index, null, DateTime.MaxValue, new TimeSpan(0, 20, 0), CacheItemPriority.Normal, new CacheItemRemovedCallback(DataCacheItemRemoved));
                WriteDiagnosticMessage(string.Format("ADD - AddToDictionary({0}): {1}", cacheKey, id));
                WriteDiagnosticMessage(string.Format("COUNT - AddToDictionary({0}): {1}", cacheKey, index.Keys.Count));
            }
        }

        /// <summary>
        /// Remove an item from a cached dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="cacheKey">Cache Key identifying the dictionary</param>
        /// <param name="id">Dictionary Id specifying the item to remove</param>
        public static void RemoveFromDictionary<TKey, TValue>(string cacheKey, TKey id)
        {
            EnsureCache();

            lock (_lockObject)
            {
                Dictionary<TKey, TValue> index = (Dictionary<TKey, TValue>)_cache.Get(cacheKey);

                if (index != null && index.ContainsKey(id))
                {
                    index.Remove(id);
                    _cache.Add(cacheKey, index, null, DateTime.MaxValue, new TimeSpan(0, 20, 0), CacheItemPriority.Normal, new CacheItemRemovedCallback(DataCacheItemRemoved));
                    WriteDiagnosticMessage(string.Format("DEL - RemoveFromDictionary({0}): {1}", cacheKey, id));
                    WriteDiagnosticMessage(string.Format("COUNT - AddToDictionary({0}): {1}", cacheKey, index.Keys.Count));
                }
            }
        }

        /// <summary>
        /// Get a single object from the cache.
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="cacheKey">Cache key identifying the object to be retrieved</param>
        /// <returns>TValue of Single Object</returns>
        public static TValue GetSingleObject<TValue>(string cacheKey)
        {
            EnsureCache();

            TValue data = default(TValue);

            if (ContainsKey(cacheKey))
            {
                lock (_lockObject)
                {
                    data = (TValue)_cache.Get(cacheKey);

                    if (data != null && !data.Equals(default(TValue)))
                    {
                        WriteDiagnosticMessage(string.Format("HIT - GetSingleObject({0})", cacheKey));
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Add a single object to the cache.
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="cacheKey">Cache Key identifying the value being added</param>
        /// <param name="data">object to be added</param>
        public static void AddSingleObject<TValue>(string cacheKey, TValue data)
        {
            EnsureCache();

            lock (_lockObject)
            {
                _cache.Remove(cacheKey);
                _keyList.Remove(cacheKey);

                _cache.Add(cacheKey, data, null, DateTime.MaxValue, new TimeSpan(0, 20, 0), CacheItemPriority.Normal, new CacheItemRemovedCallback(DataCacheItemRemoved));
                _keyList.Add(cacheKey);
                
                WriteDiagnosticMessage(string.Format("ADD - AddSingleObject({0})", cacheKey));
            }
        }

        /// <summary>
        /// Remove a specific item from the cache (either a whole entry or a single object)
        /// </summary>
        /// <param name="cacheKey">Cache Key to be removed</param>
        public static void Remove(string cacheKey)
        {
            EnsureCache();

            lock (_lockObject)
            {
                _cache.Remove(cacheKey);

                WriteDiagnosticMessage(string.Format("DEL - Remove({0})", cacheKey));
            }
        }

        /// <summary>
        /// Purge the entire Cache.
        /// </summary>
        public static void Purge()
        {
            EnsureCache();

            lock (_lockObject)
            {
                WriteDiagnosticMessage("PURGE - Start");

                IDictionaryEnumerator x = _cache.GetEnumerator();
                while (x.MoveNext())
                {
                    _cache.Remove(Convert.ToString(x.Key));
                    WriteDiagnosticMessage(string.Format("PURGE - Remove({0})", x.Key));
                }

                _keyList.Clear();

                WriteDiagnosticMessage("PURGE - Finish");
            }
        }

        /// <summary>
        /// Check if the cache currently contains a given key
        /// </summary>
        /// <param name="cacheKey">Cache Key to check for</param>
        /// <returns>bool - true if Key exists, otherwise false</returns>
        public static bool ContainsKey(string cacheKey)
        {
            EnsureCache();
            
            bool contains = false;

            lock (_lockObject)
            {
                contains = _keyList.Contains(cacheKey);
            }

            return contains;
        }

        /// <summary>
        /// Ensure that a cache object exists, and is available for use.
        /// </summary>
        private static void EnsureCache()
        {
            lock (_lockObject)
            {
                if (_httpRuntime == null)
                {
                    _httpRuntime = new HttpRuntime();
                }

                if (_cache == null)
                {
                    _cache = HttpRuntime.Cache;
                }
            }
        }
        
        /// <summary>
        /// Write out a Diagnostic message
        /// </summary>
        /// <param name="message">Messge to be written</param>
        private static void WriteDiagnosticMessage(string message)
        {
            if (_consoleOutput)
            {
                Console.Out.WriteLine(message);
            }
        }

        private static void DataCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            WriteDiagnosticMessage(string.Format("EXPIRED - {0} for reason {1}", key, reason.ToString()));

            lock (_lockObject)
            {
                _keyList.Remove(key);
            }

            WriteDiagnosticMessage(string.Format("REMOVED KEY - {0} for expiry reason {1}", key, reason.ToString()));            
        }
    }
}
