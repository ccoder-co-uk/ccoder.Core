using cCoder.Core.Data;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.CMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace cCoder.Core
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

                    log.LogInformation($"{DateTimeOffset.Now} - Processing common object cache");
                    CommonObject[] distinctTypeSets = LoadPaged(core);

                    CommonObject[] componentObjects = distinctTypeSets.Where(c => c.Type == "Core/Component").ToArray();
                    CommonObject[] resourceObjects = distinctTypeSets.Where(c => c.Type == "Core/Resource").ToArray();
                    CommonObject[] scriptObjects = distinctTypeSets.Where(c => c.Type == "Core/Script").ToArray();

                    LatestSet = componentObjects.Union(resourceObjects).Union(scriptObjects).ToArray();

                    tempSet.AddRange(resourceObjects
                        .AsParallel()
                        .WithDegreeOfParallelism(8)
                        .Select(n => Objects.Data.ParseJson<Resource>(n.Json)));
                    tempSet.AddRange(componentObjects
                        .AsParallel()
                        .WithDegreeOfParallelism(8)
                        .Select(n => Objects.Data.ParseJson<Component>(n.Json)));
                    tempSet.AddRange(scriptObjects
                        .AsParallel().WithDegreeOfParallelism(8)
                        .Select(n => Objects.Data.ParseJson<Script>(n.Json)));
                    log.LogInformation($"{DateTimeOffset.Now} - Processed common object cache");

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

        private static Func<CoreDataContext, int, int, IEnumerable<CommonObject>> compiledCommonCacheQuery =
            Microsoft.EntityFrameworkCore.EF.CompileQuery<CoreDataContext, int, int, IEnumerable<CommonObject>>
                ((CoreDataContext context, int skip, int take) => context.GetAll<CommonObject>(false)
                .GroupBy(c => new { c.Name, c.Culture, c.Key, c.Type })
                .Select(c => c.OrderByDescending(v => v.Version).First())
                .Skip(skip)
                .Take(take));

        private static CommonObject[] LoadPaged(ICoreDataContext core)
        {
            Debug.WriteLine($"{System.DateTimeOffset.Now} - Loading cache");

            int pageSize = 500;
            int i = 0;

            var context = core as CoreDataContext;

            IEnumerable<CommonObject> page = compiledCommonCacheQuery(context, i, pageSize);

            var result = new List<CommonObject>();

            while (page.Any())
            {
                result.AddRange(page);
                page = compiledCommonCacheQuery(context, i, pageSize);

                i += pageSize;
            }

            Debug.WriteLine($"{System.DateTimeOffset.Now} - Loaded cache");

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