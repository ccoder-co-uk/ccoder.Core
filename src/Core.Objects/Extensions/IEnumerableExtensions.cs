using Core.Objects.Entities.CMS;
using Core.Objects.Entities.DMS;
using Core.Objects.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Objects.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// No idea why this isn't in .Net already to be honest but never mind ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// 

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            if (source != null)
            {
                for (int i = 0; i < source.Count(); i++)
                {
                    action(source.ElementAt(i), i);
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null)
            {
                foreach (T i in source)
                {
                    action(i);
                }
            }
        }

        public static int IndexOf<T>(this IList<T> source, Func<T, bool> condition)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (condition(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static readonly JsonSerializerSettings packagingSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true }
        };

        public static Entities.DMS.File AsJsonFile<T>(this IEnumerable<T> source, string named) where T : class => new()
        {
            Name = named,
            Contents = new[]
                {
                    new FileContent { Version = 1, RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source.ToArray(), packagingSettings)) }
                }
        };

        /// <summary>
        /// Merges a collection in to another collection using an Additive merge.
        /// The source should be an entity collection from the system (exists in the db), and the "other" collection can be any other collection from any source.
        /// The known Ts are a collection of Ts that are deemed relevant for the merge operation that exist in the system (db).
        /// Along side those a data context and 2 operation functions are passed ...
        ///  1. "match" is a function to use when matching 2 rows that may have different primary keys but may also be "logically the same row".
        ///  2. "add" is a function to use to prepare a new T from the other collection that may have come from another system / environment, the add operation will be called for each T and it's result Added in to the context.
        /// </summary>
        /// <typeparam name="T">The type of the collections being merged</typeparam>
        /// <param name="source">the source collection we want the merged results to be kept in</param>
        /// <param name="otherTs">A collection of T's to merge in to the source collection</param>
        /// <param name="knownTs">A collection of "known" T's from the data context given in the "db" parameter</param>
        /// <param name="db">The data context on which this merge operation is to be ultimately taking place</param>
        /// <param name="match">A record matching expression when primary keys simply won't do</param>
        /// <param name="add">A record preparation expression to apply before adding a new T to the context</param>
        /// <returns></returns>
        public static async Task SyncWith<T>(this ICollection<T> source, IEnumerable<T> otherTs, IEnumerable<T> knownTs, IDataContext db, Func<T, T, bool> match = null, Action<T> beforeAdd = null) 
            where T : class
        {
            beforeAdd ??= (aT) =>
            {
                System.Reflection.PropertyInfo idProp = aT.GetIdProperty();
                object value = idProp.PropertyType.IsValueType ? Activator.CreateInstance(idProp.PropertyType) : null;
                idProp.SetValue(aT, value);
            };

            // we only actually act if we have something to merge with 
            if (otherTs != null)
            {
                knownTs ??= new List<T>();

                // New Ts added from otherTs that are in the db
                // we must get the subset from otherTs that ARE NOT in source (So new) and also ARE in DB
                List<T> addedKnownTs = (match != null
                    ? otherTs
                        .Where(ot => !source.Any(s => match(s, ot)))
                        .Where(t => knownTs.Any(kt => match(t, kt)))
                    : otherTs
                        .Where(ot => !source.Any(s => s.GetId().ToString() == knownTs.GetId().ToString()))
                        .Where(t => knownTs.Any(kt => kt.GetId().ToString() == t.GetId().ToString()))
                )
                .ToList();

                // New Ts that are not in the db 
                List<T> newTs = otherTs
                    .Where(ot => !addedKnownTs.Contains(ot) && !source.Any(t => match != null
                        ? match(t, ot)
                        : t.GetId().ToString() == ot.GetId().ToString())
                    )
                    .ToList();

                // add the new ones that the db doesn't know about to the db then to the addedKnownTs collection
                foreach (T nt in newTs)
                {
                    beforeAdd(nt);
                    addedKnownTs.Add(await db.AddAsync(nt));
                }

                // perform adds 
                addedKnownTs.ForEach(i => source.Add(i));
            }
        }

        /// <summary>
        /// Breaks a collection of items in to sub collections / batches.
        /// </summary>
        /// <typeparam name="T">Type of the collection to break up</typeparam>
        /// <param name="source">the collection</param>
        /// <param name="chunkSize">The size of the collections / batches that will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> BatchesOf<T>(this IEnumerable<T> source, int chunkSize) => source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

        public static string DisplayNameFor(this IEnumerable<Resource> source, string resourceName)
            => (source.FirstOrDefault(r => r.Name == resourceName) ?? new Resource { DisplayName = $"[{resourceName}_DisplayName]" }).DisplayName;

        public static string ShortDisplayNameFor(this IEnumerable<Resource> source, string resourceName)
            => (source.FirstOrDefault(r => r.Name == resourceName) ?? new Resource { ShortDisplayName = $"[{resourceName}_ShortDisplayName]" }).ShortDisplayName;

        public static string DescriptionFor(this IEnumerable<Resource> source, string resourceName)
            => (source.FirstOrDefault(r => r.Name == resourceName) ?? new Resource { Description = $"[{resourceName}_Description]" }).Description;

        public static async Task Match<T, T2>(this IEnumerable<T> source, IEnumerable<T2> possibles, Func<T, T2, bool> with, Func<T, T2, Task> then)
        {
            foreach (T i in source)
                await then(i, possibles.FirstOrDefault(j => with(i, j)));
        }
    }
}