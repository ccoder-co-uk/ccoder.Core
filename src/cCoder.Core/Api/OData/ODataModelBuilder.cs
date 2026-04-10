using System.Linq.Expressions;
using cCoder.Core.Models;
using cCoder.Data.Extensions;
using cCoder.Core.Models.Metadata;
using Microsoft.OData.ModelBuilder;


namespace cCoder.Core.Api.OData;

/// <summary>
/// Base model builder class for all OData model builders
/// </summary>
public abstract class ODataModelBuilder
{
    protected ODataConventionModelBuilder Builder = new();

    /// <summary>
    /// Derived types implement this to setup the OData Model information
    /// </summary>
    /// <returns></returns>
    public abstract ODataModel Build();

    protected virtual EntitySetConfiguration<T> AddSet<T, TKey>(
        bool enableBatchingToo = false,
        string setName = null
    )
        where T : class
    {
        setName ??= typeof(T).Name;
        return Builder.EntitySet<T>(setName);
    }

    protected virtual EntitySetConfiguration<T> AddJoinSet<T, TKey>(Expression<Func<T, TKey>> key)
        where T : class
    {
        string setName = typeof(T).Name;
        EntitySetConfiguration<T> setConfig = Builder.EntitySet<T>(setName);
        _ = Builder.EntityType<T>().HasKey(key);

        return setConfig;
    }

    /// <summary>
    /// Used by shared metadata and lookup support
    /// </summary>
    protected virtual void AddCommonComplextypes()
    {
        _ = Builder.ComplexType<MetadataContainerSet>();
        _ = Builder.ComplexType<MetadataContainer>();
        _ = Builder.ComplexType<PropertyContainer>();
        _ = Builder.ComplexType<AuditResultsByUser>();
        _ = Builder.ComplexType<AuditResultByProperty>();
    }
}
