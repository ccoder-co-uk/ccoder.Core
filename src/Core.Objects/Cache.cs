using Core.Objects.Dtos;
using Core.Objects.Extensions;
using System.Collections.Concurrent;
using System.Timers;

namespace Core.Objects
{
    public class Cache<T> : ICache<T>
    {
        public int ExpiryTimeInMinutes { get; set; }

        protected ConcurrentDictionary<CacheKey, object> Data = new();
        protected readonly System.Timers.Timer timer = new();

        protected struct CacheKey
        {
            public string Key { get; set; }
            public DateTime AddedOn { get; set; }
        }

        public Cache()
        {
            ExpiryTimeInMinutes = 10;
            timer.Elapsed += ScanForExpiredItems;
            timer.Start();
        }

        private void ScanForExpiredItems(object sender, ElapsedEventArgs e)
        {
            lock (Data)
            {
                IEnumerable<CacheKey> expiredKeys = Data.Keys.Where(i => i.AddedOn < DateTime.Now.AddMinutes(-ExpiryTimeInMinutes));
                foreach (CacheKey key in expiredKeys)
                {
                    _ = Data.TryRemove(key, out object lastRemoval);
                }
            }
        }

        public T Get(string key)
        {
            CacheKey cacheKey = Data.Keys.FirstOrDefault(i => i.Key == key.ToLower());
            return cacheKey.Key != null ? (T)Data[cacheKey] : default;
        }

        public void Set(string key, T item)
        {

            CacheKey cacheKey = new() { Key = key.ToLower(), AddedOn = DateTime.Now };
            _ = Data.AddOrUpdate(cacheKey, item, (_, _) => item);
        }

        internal IEnumerable<Replacement> AsReplacementSet(string tagName) => Data.Select(i => new Replacement($"[{tagName}[{i.Key.Key}]]", i.Value.ToJson())).ToArray();

        public string ToJson() => Data.Values.ToJson();

        public IDictionary<string, T> ToDictionary()
        {
            Dictionary<string, T> result = new();
            Data.ForEach(i => result.Add(i.Key.Key, (T)i.Value));
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Dispose();
                Data.Clear();
                Data = null;
            }
        }
    }
}