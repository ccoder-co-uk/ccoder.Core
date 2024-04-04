using cCoder.Core.Api.OData;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Dtos.Workflow;
using cCoder.Core.Objects.Workflow.Activities;
using Microsoft.OData.Edm;
using System.Reflection;

namespace cCoder.Core.Api
{
    public static class MetadataHelper
    {
        static IQueryable<MetadataContainerSet> cache = null;

        public static IQueryable<MetadataContainerSet> MetaForEverything()
        {
            if (cache == null)
            {
                cache = SystemTypes()
                    .Union(EntityTypes())
                    .Union(new[] { WorkflowTypes() })
                    .Union(DTOs())
                    .OrderBy(s => s.Name)
                    .AsQueryable();
            }

            return cache;
        }

        private static IEnumerable<MetadataContainerSet> DTOs()
        {
            return new[]
            {
                new MetadataContainerSet
                {
                    Name = "DTOs",
                    Types = new[] {
                        new MetadataContainer(typeof(Flow)) { Category = "DTO (Workflow)" },
                        new MetadataContainer(typeof(Link)) { Category = "DTO (Workflow)" },
                        new MetadataContainer(typeof(WorkflowLogEntry)) { Category = "DTO (Workflow)" },
                        new MetadataContainer(typeof(WorkflowLogLevel)) { Category = "DTO (Workflow)" }
                    }
                    .Union(
                        TypeHelper.GetWebStackAssemblies()
                            .SelectMany(a => a.GetTypes()
                                .Where(t => t.Namespace == "B2B.Objects.Dtos"))
                                .Select(t => new MetadataContainer(t) { Category = "DTO (B2B)" })
                    )
                    .Union(
                        TypeHelper.GetWebStackAssemblies()
                            .SelectMany(a => a.GetTypes()
                                .Where(t => t.Namespace == "cCoder.Core.Objects"))
                                .Select(t => new MetadataContainer(t) { Category = "DTO (cCoder.Core)" })
                    )
                    .OrderBy(t => t.Name)
                    .ToArray()
                }
            };
        }

        static MetadataContainerSet WorkflowTypes() => new()
        {
            Name = "Workflow",
            Types = TypeHelper.GetWebStackAssemblies()
                .SelectMany(a => a.GetTypes()
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

        static IEnumerable<MetadataContainerSet> SystemTypes() => new[] {
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

        static IEnumerable<MetadataContainerSet> EntityTypes()
        {
            Dictionary<string, IEdmModel> map = new()
            {
                { "Core", new CoreModelBuilder().Build().EDMModel }
                //{ "Security", new SecurityModelBuilder().Build().EDMModel },
            };

            return TypeHelper.GetContextTypes()
                .Select(ctxType =>
                {
                    string name = ctxType.Name.Replace("DataContext", "");
                    IEdmModel model = map.ContainsKey(name) ? map[name] : null;

                    return new MetadataContainerSet
                    {
                        Name = name,
                        UriBase = name,
                        Types = TypeHelper.GetEntityTypesFor(ctxType).Select(entType =>
                        {
                            bool hasEndpoint = entType.GetCustomAttribute<ApiIgnoreAttribute>() == null;
                            return model != null
                                ? model.GetExtendedMetadataForType(name, entType, hasEndpoint)
                                : new ExtendedMetadataContainer(entType, true, hasEndpoint) { Category = name };
                        })
                        .OrderBy(t => t.Name)
                        .ToArray()
                    };
                });
        }
    }
}