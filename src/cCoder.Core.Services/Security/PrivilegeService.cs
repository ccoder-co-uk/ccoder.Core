using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.Security
{
    public class PrivilegeService : CoreService<Privilege>
    {
        public PrivilegeService(ICoreDataContext db) : base(db) { }

        public override Task<Privilege> AddAsync(Privilege entity)
        {
            if (!User.Can(null, "privilege_create"))
                throw new SecurityException("Access Denied!");

            throw new InvalidOperationException("Cannot add privileges");
        }

        public override Task<Privilege> UpdateAsync(Privilege entity)
        {
            if (!User.Can(null, "privilege_update"))
                throw new SecurityException("Access Denied!");

            throw new InvalidOperationException("Cannot update privileges");
        }

        public override Task DeleteAsync(object id)
        {
            if (!User.Can(null, "privilege_delete"))
                throw new SecurityException("Access Denied!");

            throw new InvalidOperationException("Cannot delete privileges");
        }

        public override IQueryable<Privilege> GetAll()
        {
            if (!User.Can(null, "privilege_read"))
                throw new SecurityException("Access Denied!");

            return Db.GetAllPrivileges().AsQueryable();
        }

        public override Privilege Get(object id)
        {
            if (!User.Can(null, "privilege_read"))
                throw new SecurityException("Access Denied!");

            return Db.GetAllPrivileges().FirstOrDefault(f => f.Id == (string)id);
        }
    }
}