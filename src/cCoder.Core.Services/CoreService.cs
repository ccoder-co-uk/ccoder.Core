using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using System.Security;

namespace cCoder.Core.Services;

public abstract class CoreService<T> : Service<T, User>, ICoreService<T> where T : class
{
    protected new ICoreDataContext Db => base.Db as ICoreDataContext;

    protected CoreService(ICoreDataContext db) : base(db) { }

    public override T Get(object id) =>
        User.Can(null, typeof(T).Name + "_read")
            ? base.Get(id)
            : throw new SecurityException("Access Denied!");

    public override IQueryable<T> GetAll() =>
       User.Can(null, typeof(T).Name + "_read")
            ? base.GetAll()
            : throw new SecurityException("Access Denied!");

    public override Task<T> AddAsync(T entity) =>
        User.Can(Db.GetAppId(entity), typeof(T).Name + "_create")
            ? base.AddAsync(entity)
            : throw new SecurityException("Access Denied!");

    public override Task<T> UpdateAsync(T entity) =>
        User.Can(Db.GetAppId(entity), typeof(T).Name + "_update")
            ? base.UpdateAsync(entity)
            : throw new SecurityException("Access Denied!");

    public override Task DeleteAsync(object id) =>
        User.Can(Db.GetAppId(Get(id)), typeof(T).Name + "_delete")
            ? base.DeleteAsync(id)
            : throw new SecurityException("Access Denied!");

    public override async Task DeleteAllAsync(IEnumerable<T> items)
    {
        bool userCan = items.All(i => User.Can(Db.GetAppId(i), typeof(T).Name + "_delete"));

        if (userCan)
            await base.DeleteAllAsync(items);
        else
            throw new SecurityException("Access Denied!");
    }
}