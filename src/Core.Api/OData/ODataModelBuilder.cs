using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Dtos.Metadata;
using Core.Objects.Extensions;
using Microsoft.OData.ModelBuilder;
using System.Linq.Expressions;

namespace Core.Api.OData
{
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

        protected virtual EntitySetConfiguration<T> AddSet<T, TKey>(bool enableBatchingToo = false, string setName = null) where T : class
        {
            setName ??= typeof(T).Name;
            _ = Builder.EntitySet<Result<T>>(setName + "Results");
            EntitySetConfiguration<T> setConfig = Builder.EntitySet<T>(setName);

            // register base OData controller defined functions
            _ = Builder.EntityType<T>().Collection.Function("GetMetadata").Returns<MetadataContainer>();
            _ = Builder.EntityType<T>().Collection.Action("AddAll").ReturnsCollectionFromEntitySet<Result<T>>(setName + "Results");
            _ = Builder.EntityType<T>().Collection.Action("UpdateAll").ReturnsCollectionFromEntitySet<Result<T>>(setName + "Results");
            _ = Builder.EntityType<T>().Collection.Action("DeleteAll").ReturnsCollection<Result<TKey>>();
            _ = Builder.EntityType<T>().Collection.Action("AddOrUpdateAll").ReturnsCollectionFromEntitySet<Result<T>>(setName + "Results");

            System.Reflection.PropertyInfo[] removedProps = typeof(T).GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ApiIgnoreAttribute)))
                .ToArray();

            StructuralTypeConfiguration typeInfo = Builder.StructuralTypes.First(t => t.ClrType == typeof(T));
            removedProps.ForEach(p => typeInfo.RemoveProperty(p));

            return setConfig;
        }

        protected virtual EntitySetConfiguration<T> AddJoinSet<T, TKey>(Expression<Func<T, TKey>> key) where T : class
        {

            string setName = typeof(T).Name;
            // register basic CRUD endpoint
            EntitySetConfiguration<T> setConfig = Builder.EntitySet<T>(setName);
            _ = Builder.EntitySet<Result<T>>(setName + "Results");

            // register base OData controller defined functions
            _ = Builder.EntityType<T>().Collection.Function("GetMetadata").Returns<MetadataContainer>();
            _ = Builder.EntityType<T>().Collection.Action("AddAll").ReturnsCollectionFromEntitySet<Result<T>>(setName + "AddResults");
            _ = Builder.EntityType<T>().Collection.Action("DeleteAll");

            _ = Builder.EntityType<T>().HasKey(key);

            System.Reflection.PropertyInfo[] removedProps = typeof(T).GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(ApiIgnoreAttribute)))
                .ToArray();

            StructuralTypeConfiguration typeInfo = Builder.StructuralTypes.First(t => t.ClrType == typeof(T));
            removedProps.ForEach(p => typeInfo.RemoveProperty(p));

            return setConfig;
        }

        /// <summary>
        /// Used by the generic functions GetMetadata and Lookup
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
}