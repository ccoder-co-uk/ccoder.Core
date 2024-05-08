using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;

namespace cCoder.Core.Services.Packaging.Importers;

public abstract class Importer<T, TUser> : IPackageItemImporter where T : class
{
    public string Type { get; }

    public int Order { get; protected set; } = 1;

    protected IService<T, TUser> Service { get; }

    protected Importer(IService<T, TUser> service, string type)
    {
        Type = type;
        Service = service;
    }

    public abstract Task Import(int appId, PackageItem item);
}