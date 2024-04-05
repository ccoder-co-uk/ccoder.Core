using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services
{
    public abstract class Service<T, TUser> : Service<TUser>, IService<T, TUser> where T : class
    {
        public virtual TUser User => Db.User;

        protected virtual bool UserIsActive => User != null;

        protected Service(IDataContext<TUser> db) : base(db) { }

        public virtual T Get(object id)
            => UserIsActive ? Db.Get<T>(id) : throw new SecurityException("Access Denied!");

        public virtual IQueryable<T> GetAll()
            => (UserIsActive) ? Db.GetAll<T>(false) : throw new SecurityException("Access Denied!");

        public virtual IQueryable<T> GetAll(bool andTrack)
            => (UserIsActive) ? Db.GetAll<T>(andTrack) : throw new SecurityException("Access Denied!");

        public virtual async Task<T> UpdateAsync(T entity)
            => (UserIsActive) ? await Db.UpdateAsync(entity) : throw new SecurityException("Access Denied!");

        public virtual Task DeleteAsync(object key)
            => (UserIsActive) ? Db.DeleteAsync(Db.Get<T>(key)) : throw new SecurityException("Access Denied!");

        public virtual async Task<T> AddAsync(T entity)
            => (UserIsActive) ? await Db.AddAsync(entity) : throw new SecurityException("Access Denied!");

        public virtual async Task<IEnumerable<Result<T>>> AddAllAsync(IEnumerable<T> items)
        {
            List<Result<T>> results = new();

            // call the most specific instance of add for each to ensure all the add rules get implemented
            foreach (T item in items)
            {
                try
                {
                    results.Add(new Result<T> { Success = true, Item = await AddAsync(item), Message = "Added Successfully" });
                }
                catch (Exception ex)
                {
                    results.Add(new Result<T> { Success = false, Item = item, Message = ex.Message });
                }
            }

            return results;
        }

        public virtual async Task<IEnumerable<Result<T>>> UpdateAllAsync(IEnumerable<T> items)
        {
            List<Result<T>> results = new();

            // call the most specific instance of add for each to ensure all the add rules get implemented
            foreach (T item in items)
            {
                try
                {
                    results.Add(new Result<T> { Success = true, Item = await UpdateAsync(item), Message = "Updated Successfully" });
                }
                catch (Exception ex)
                {
                    results.Add(new Result<T> { Success = false, Item = item, Message = ex.Message });
                }
            }

            return results;
        }

        public virtual async Task DeleteAllAsync(IEnumerable<T> items)
        {
            if (UserIsActive)
                await Db.DeleteAllAsync(items);
            else
                throw new SecurityException("Access Denied!");
        }

        public virtual async Task<IEnumerable<Result<T>>> AddOrUpdate(IEnumerable<T> items, bool onlyIfNewer = true)
        {
            object[] ids = items.Select(i => i.GetId()).ToArray();

            System.Reflection.MethodInfo whereIdInBuilder = typeof(TypeExtensions)
                .GetMethod("WhereIdIn", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .MakeGenericMethod(typeof(T));

            Func<T, bool> whereIdIn = ((Expression<Func<T, bool>>)whereIdInBuilder.Invoke(null, new object[] { typeof(T), ids }))
                .Compile();

            //whereIdIn tends to look like (x => ids.Contains(x.Id))
            // get everything removing security
            Db.DisableFilters();
            T[] allDbVersions = GetAll(false).Where(whereIdIn).ToArray();

            // get what the user has access to 
            Db.EnableFilters();
            T[] visibleDbVersions = GetAll(false).Where(whereIdIn).ToArray();

            // figure out if any of the items are there but the ser can't see them
            // add those as the initial set of rejections.
            List<Result<T>> results = items.Where(i =>
            {
                T allVer = allDbVersions.FirstOrDefault(typeof(T).IdEquals<T>(i.GetId()).Compile());
                T visibleVer = visibleDbVersions.FirstOrDefault(typeof(T).IdEquals<T>(i.GetId()).Compile());
                return allVer != null && visibleVer == null;
            }).Select(i => new Result<T> { Success = false, Item = i, Message = "Access Denied!" })
            .ToList();

            // split out the workload in to sets to add and sets to update
            T[] rejectedItems = results.Select(i => i.Item).ToArray();
            T[] existingItems = items.Where(i => !rejectedItems.Contains(i) && visibleDbVersions.Any(dbi => dbi.GetId().ToString() == i.GetId().ToString())).ToArray();
            T[] newItems = items.Where(i => !rejectedItems.Contains(i) && !existingItems.Contains(i)).ToArray();

            if (onlyIfNewer)
            {
                System.Reflection.PropertyInfo lastUpdated = typeof(T).GetProperty("LastUpdated");

                if (lastUpdated == null || lastUpdated.PropertyType != typeof(DateTimeOffset))
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not define the last time it was updated with a LastUpdated property of type DateTimeOffset");

                existingItems = existingItems
                    .Where(i => (DateTimeOffset)lastUpdated.GetValue(i) > (DateTimeOffset)lastUpdated.GetValue(visibleDbVersions.First(dbi => dbi.GetId().ToString() == i.GetId().ToString()))).ToArray();
            }

            // do the work adding the result sets to our final set ...
            results.AddRange(await UpdateAllAsync(existingItems));
            results.AddRange(await AddAllAsync(newItems));

            // and we are done!
            return results;
        }
    }
}