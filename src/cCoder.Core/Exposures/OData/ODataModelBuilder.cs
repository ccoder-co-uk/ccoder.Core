// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Linq.Expressions;
using cCoder.Core.Models;
using cCoder.Data.Extensions;
using cCoder.Core.Models.Metadata;
using Microsoft.OData.ModelBuilder;


namespace cCoder.Core.Exposures.OData;

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
        return Builder.EntitySet<T>(name: setName);
    }

    protected virtual EntitySetConfiguration<T> AddJoinSet<T, TKey>(Expression<Func<T, TKey>> key)
        where T : class
        =>
        (
            Set: Builder.EntitySet<T>(name: typeof(T).Name),
            Key: Builder.EntityType<T>()
                .HasKey(keyDefinitionExpression: key)
        )
            .Set;

    protected virtual void AddCommonComplextypes()
    {
        _ = new object[]
        {
            Builder.ComplexType<MetadataContainerSet>(),
            Builder.ComplexType<MetadataContainer>(),
            Builder.ComplexType<PropertyContainer>(),
            Builder.ComplexType<AuditResultsByUser>(),
            Builder.ComplexType<AuditResultByProperty>()
        };
    }
}