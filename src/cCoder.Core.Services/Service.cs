using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos;
using System;

namespace cCoder.Core.Services
{
    /// <summary>
    /// Base class for all business services 
    /// </summary>
    public abstract class Service<TUser> : IService
    {
        public ICrypto<Signature> Crypto { get; set; }

        protected IDataContext<TUser> Db { get; private set; }

        public ICoreAuthInfo AuthInfo => Db.AuthInfo;

        protected Service(IDataContext<TUser> db)
        {
            Db = db;
        }

        public void SetAuth(ICoreAuthInfo auth) => Db.SetAuth(auth);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }
    }
}