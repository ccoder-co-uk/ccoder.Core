using cCoder.Core.Objects.Entities.Security;

namespace cCoder.Core.Services.Packaging.Importers;

public abstract class CoreImporter<T> : Importer<T, User> where T : class
{
    protected CoreImporter(ICoreService<T> service, string type) : base(service, type) { }
}