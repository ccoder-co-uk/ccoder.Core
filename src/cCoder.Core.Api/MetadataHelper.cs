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
                .Union(DTOs())
                .OrderBy(s => s.Name)
                .AsQueryable();
        }

        return cache;
    }

    private static IEnumerable<MetadataContainerSet> DTOs() =>
    [
        new MetadataContainerSet
        {
            Name = "DTOs",
            Types = new[] {
                new MetadataContainer(typeof(Flow)) { Category = "DTO (Workflow)" },
                new MetadataContainer(typeof(Link)) { Category = "DTO (Workflow)" },
                new MetadataContainer(typeof(WorkflowLogEntry)) { Category = "DTO (Workflow)" },
                new MetadataContainer(typeof(WorkflowLogLevel)) { Category = "DTO (Workflow)" }
            }
            .OrderBy(t => t.Name)
            .ToArray()
        }
    ];

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

                    return new MetadataContainer(type) { Category = g.Key };
                }))
            .OrderBy(t => t.Name)
            .ToArray()
    };

    private static IEnumerable<MetadataContainerSet> SystemTypes() => new[] {
            new MetadataContainerSet
            {
                Name = "System",
                Types = new [] {
                    new MetadataContainer(typeof(int)),
                    new MetadataContainer(typeof(string)),
                    new MetadataContainer(typeof(decimal)),
                    new MetadataContainer(typeof(double)),
                    new MetadataContainer(typeof(float)),
                    new MetadataContainer(typeof(bool)),
                    new MetadataContainer(typeof(DateTime)),
                    new MetadataContainer(typeof(DateTimeOffset)),
                    new MetadataContainer(typeof(TimeSpan)),
                    new MetadataContainer(typeof(IEnumerable<object>)),
                    new MetadataContainer(typeof(ICollection<object>)),
                    new MetadataContainer(typeof(IDictionary<string, object>)),
                    new MetadataContainer(typeof(object)),
                    new MetadataContainer(typeof(Guid))
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