using Core.Objects;
using Core.Objects.Entities;
using Core.Objects.Entities.CMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class CommonObjectCache : Cache<object>, ICommonObjectCache
    {
        readonly ILogger log;

        protected Config Config { get; }

        public IEnumerable<CommonObject> LatestSet { get; set; }

        readonly IServiceProvider serviceProvider;

        public CommonObjectCache(Config config, IServiceProvider serviceProvider, ILogger<CommonObjectCache> log) : base()
        {
            Config = config;
            this.serviceProvider = serviceProvider;
            this.log = log;

            ExpiryTimeInMinutes = config.Settings.ContainsKey("CacheExpiry") 
                ? int.Parse(config.Settings["CacheExpiry"]) 
                : 30;

            timer.Elapsed += (_, _) => Refresh();
            timer.Interval = ExpiryTimeInMinutes * 60 * 1000;
        }

        public virtual void Refresh()
        {
            if (Config.Settings.ContainsKey("CacheSource") && Config.Settings.ContainsKey("CacheSourceAppId"))
            {
                List<object> tempSet = new();

                try
                {
                    using ICoreDataContext core = serviceProvider.CreateScope().ServiceProvider.GetService(typeof(ICoreDataContext)) as ICoreDataContext;
                    core.DisableFilters();

                    log.LogInformation("Processing common object cache");
                    CommonObject[] commonObjects = LoadPaged(core);
                    CommonObject[] distinctTypeSets = commonObjects
                        .GroupBy(c => new { c.Name, c.Culture, c.Key, c.Type })
                        .Select(group => group.ToArray().OrderByDescending(i => i.Version).First())
                        .ToArray();

                    CommonObject[] componentObjects = distinctTypeSets.Where(c => c.Type == "Core/Component").ToArray();
                    CommonObject[] resourceObjects = distinctTypeSets.Where(c => c.Type == "Core/Resource").ToArray();
                    CommonObject[] scriptObjects = distinctTypeSets.Where(c => c.Type == "Core/Script").ToArray();

                    LatestSet = componentObjects.Union(resourceObjects).Union(scriptObjects).ToArray();

                    tempSet.AddRange(resourceObjects.Select(n => Objects.Data.ParseJson<Resource>(LatestSet.First(c => c.Type == "Core/Resource" && c.Name == n.Name && c.Culture == n.Culture && c.Key == n.Key).Json)));
                    tempSet.AddRange(componentObjects.Select(n => Objects.Data.ParseJson<Component>(LatestSet.First(c => c.Type == "Core/Component" && c.Name == n.Name && c.Culture == n.Culture && c.Key == n.Key).Json)));
                    tempSet.AddRange(scriptObjects.Select(n => Objects.Data.ParseJson<Script>(LatestSet.First(c => c.Type == "Core/Script" && c.Name == n.Name && c.Culture == n.Culture && c.Key == n.Key).Json)));
                    log.LogInformation("Processed common object cache");

                    core.EnableFilters();
                    core.Dispose();
                }
                catch (Exception ex)
                {
                    log.LogError($"{ex.Message} - {ex.StackTrace}");
                }


                Data.Clear();
                tempSet.ForEach(i =>
                {
                    switch (i)
                    {
                        case Resource r:
                            Set($"resource|{r.Key?.ToLower() ?? string.Empty}-{r.Name?.ToLower() ?? string.Empty}-{r.Culture?.ToLower() ?? string.Empty}", r);
                            break;
                        case Component c:
                            Set($"component|{c.Name.ToLower()}", c);
                            break;
                        case Script s:
                            Set($"script|{s.Name.ToLower()}", s);
                            break;
                    }
                });

            }
        }

        private static CommonObject[] LoadPaged(ICoreDataContext core)
        {
            int pageSize = 500;
            int i = 0;
            var page = core.GetAll<CommonObject>(false).Skip(i).Take(pageSize).ToArray();

            var result = new List<CommonObject>();

            while (page.Any())
            {
                result.AddRange(page);
                page = core.GetAll<CommonObject>(false).Skip(i).Take(pageSize).ToArray();
                i += pageSize;
            }

            return result.ToArray();
        }

        public T[] GetAll<T>()
            => Data.AsParallel().Where(i => i.Key.Key.StartsWith(typeof(T).Name.ToLower())).Select(i => (T)i.Value).ToArray();

        public T Get<T>(string key)
        {
            object item = Get(key.ToLower());
            return item != null ? (T)item : default;
        }
    }
}