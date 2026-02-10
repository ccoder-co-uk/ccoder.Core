using cCoder.Core.Api.OData;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Workflow.Activities;
using Microsoft.OData.Edm;
using System.Reflection;

namespace cCoder.Core.Api;

public static class MetadataHelper
{
    private static IQueryable<MetadataContainerSet> cache = null;

    public static IQueryable<MetadataContainerSet> MetaForEverything(IDictionary<string, IEdmModel> map)
    {
        if (cache == null)
        {
            cache = SystemTypes()
                .Union(EntityTypes(map))
                .Union([WorkflowTypes()])
                .OrderBy(s => s.Name)
                .AsQueryable();
        }

        return cache;
    }

    private static MetadataContainerSet WorkflowTypes() => new()
    {
        Name = "Workflow",
        Types = TypeHelper.GetWebStackAssemblies()
            .SelectMany(a => a.GetExportedTypes()
            .Where(t => t.IsSubclassOf(typeof(Activity)) && t != typeof(Activity)))
            .GroupBy(t => t.BaseType.Name.Split('`')[0])
            .SelectMany(g => g.Where(t => !t.IsAbstract)
                .Select(t =>
                {
                    Type type = t.IsGenericType
                        ? t.MakeGenericType(t.GetTypeInfo().GenericTypeParameters.Select(i => typeof(object)).ToArray())
                        : t;

                    return new ExtendedMetadataContainer(type) { Category = g.Key };
                }))
            .Union(
            [
                new ExtendedMetadataContainer(typeof(Flow)) { Category = "Workflow" },
                new ExtendedMetadataContainer(typeof(Link)) { Category = "Workflow" },
                new ExtendedMetadataContainer(typeof(WorkflowLogEntry)) { Category = "Workflow" },
                new ExtendedMetadataContainer(typeof(WorkflowLogLevel)) { Category = "Workflow" }
            ])
            .OrderBy(t => t.Name)
            .ToArray()
    };

    private static IEnumerable<MetadataContainerSet> SystemTypes() => new[] {
            new MetadataContainerSet
            {
                Name = "System",
                Types = new [] {
                    new ExtendedMetadataContainer(typeof(int)),
                    new ExtendedMetadataContainer(typeof(string)),
                    new ExtendedMetadataContainer(typeof(decimal)),
                    new ExtendedMetadataContainer(typeof(double)),
                    new ExtendedMetadataContainer(typeof(float)),
                    new ExtendedMetadataContainer(typeof(bool)),
                    new ExtendedMetadataContainer(typeof(DateTime)),
                    new ExtendedMetadataContainer(typeof(DateTimeOffset)),
                    new ExtendedMetadataContainer(typeof(TimeSpan)),
                    new ExtendedMetadataContainer(typeof(IEnumerable<object>)),
                    new ExtendedMetadataContainer(typeof(ICollection<object>)),
                    new ExtendedMetadataContainer(typeof(IDictionary<string, object>)),
                    new ExtendedMetadataContainer(typeof(object)),
                    new ExtendedMetadataContainer(typeof(Guid))
                }.Select(t => { t.Category = "System"; return t; }).ToArray()
            }
        };

    private static IEnumerable<MetadataContainerSet> EntityTypes(IDictionary<string, IEdmModel> map)
    {
        map.Add("Core", new CoreModelBuilder().Build().EDMModel);

        return map.Select(modelItem =>
        {
            return new MetadataContainerSet
            {
                Name = modelItem.Key,
                UriBase = modelItem.Key,
                Types = [.. modelItem.Value.GetMetadata(modelItem.Key).OrderBy(t => t.Name)]
            };
        });
    }
}