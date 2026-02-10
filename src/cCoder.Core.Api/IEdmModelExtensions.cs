using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Api;

public static class IEdmModelExtensions
{
    public static IEnumerable<ExtendedMetadataContainer> GetMetadata(this IEdmModel model, string contextName)
    {
        List<ExtendedMetadataContainer> types = [];

        foreach (var entitySet in model.EntityContainer.EntitySets())
        {
            var clr = GetClrType(model, entitySet.EntityType);

            if (clr != null)
                types.Add(GetExtendedMetadataForType(model, contextName, clr, hasEndpoint: true));
        }

        foreach (var schemaType in model.SchemaElements.OfType<IEdmSchemaType>())
        {
            if (schemaType is IEdmComplexType || schemaType is IEdmEntityType)
            {
                var clr = GetClrType(model, schemaType);

                if (clr != null)
                {
                    bool hasEndpoint = model.EntityContainer.FindEntitySet(clr.Name) != null;
                    types.Add(GetExtendedMetadataForType(model, contextName, clr, hasEndpoint));
                }
            }
        }

        return types.DistinctBy(t => t.ServerTypeName);
    }

    public static ExtendedMetadataContainer GetExtendedMetadataForType(this IEdmModel model, string context, Type type, bool hasEndpoint = true)
    {
        ExtendedMetadataContainer result = new(type, true, hasEndpoint) 
        { 
            Category = context 
        };

        IEdmEntitySet set = model.EntityContainer.FindEntitySet(type.Name);

        string[] exclusions = type.GetCustomAttributes(true)
            .Where(a => a is ApiIgnoreOperationAttribute)
            .Select(a => ((ApiIgnoreOperationAttribute)a).Operation)
            .ToArray();

        if (set != null)
        {
            IEnumerable<OperationContainer> customOperations = model.FindDeclaredBoundOperations(set.Type)
                .Select(o => new OperationContainer
                {
                    Name = o.Name,
                    Url = $"{result.Category}/{type.Name}/{o.Name}()",
                    Queryable = o.IsFunction(),
                    HttpVerb = o.IsFunction() ? "GET" : "POST",
                    ReturnType = BuildMetaFor(o.ReturnType?.Definition),
                    Parameters = o.Parameters?
                        .Where(p => p.Name != "bindingParameter")
                        .Select(p => new { k = p.Name, v = p.Type.FullName() })
                        .ToDictionary(i => i.k, i => i.v)
                });

            result.Operations = GetBaseCRUDOperations(result)
                .Union(customOperations)
                .Where(o => !exclusions.Contains(o.Name))
                .ToList();
        }
        else
            result.HasEndpoint = false;

        return result;
    }

    static Type GetClrType(IEdmModel model, IEdmSchemaType edmType) =>
        model.GetAnnotationValue<ClrTypeAnnotation>(edmType)?.ClrType;

    private static MetadataContainer BuildMetaFor(IEdmType definition)
    {
        if (definition != null && definition.TypeKind == EdmTypeKind.Collection)
        {
            Type cSharpType = Type.GetType(definition.FullTypeName(), false);

            if (cSharpType != null)
                return new MetadataContainer(cSharpType, true, true);
        }

        return null;
    }

    private static IEnumerable<OperationContainer> GetBaseCRUDOperations(MetadataContainer type) => type.IsJoinEntity 
        ? GetBaseCRUDOperationsForJoinEntity(type) 
        : GetBaseCRUDOperationsForEntity(type);

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForJoinEntity(MetadataContainer type) =>
    [
        new() 
        {
            Name = "Add",
            Url = $"{type.Category}/{type.Name}",
            Queryable = true,
            HttpVerb = "POST",
            ReturnType = type,
            Parameters = new Dictionary<string, string>
            {
                { "body:entity", type.ServerType }
            }
        },
        new() 
        {
            Name = "Get",
            Url = $"{type.Category}/{type.Name}({{Left=leftKey,Right=rightKey}})",
            Queryable = true,
            HttpVerb = "GET",
            ReturnType = type,
            Parameters = new Dictionary<string, string>
            {
                { "odata:key", Type.GetType(type.ServerType).GetIdProperty().GetType().FullName }
            }
        },
        new() 
        { 
            Name = "Get All", 
            Url = $"{type.Category}/{type.Name}", 
            Queryable = true, 
            HttpVerb = "GET", 
            ReturnType = type 
        },
        new() 
        { 
            Name = "Delete", 
            Url = $"{type.Category}/{type.Name}({{Left=leftKey,Right=rightKey}})", 
            HttpVerb = "DELETE" 
        },
    ];

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForEntity(MetadataContainer type)
    {
        return
        [
            new()
            {
                Name = "Add",
                Url = $"{type.Category}/{type.Name}",
                Queryable = true,
                HttpVerb = "POST",
                ReturnType = type,
                Parameters = new Dictionary<string, string>
                {
                    { "body:entity", type.ServerType }
                }
            },
            new()
            {
                Name = "Update",
                Url = $"{type.Category}/{type.Name}({{key}})",
                Queryable = true,
                HttpVerb = "PUT",
                ReturnType = type,
                Parameters = new Dictionary<string, string>
                {
                    { "odata:key", Type.GetType(type.ServerType).GetIdProperty()?.GetType().FullName },
                    { "body:entity", type.ServerType }
                }
            },
            new()
            {
                Name = "Get",
                Url = $"{type.Category}/{type.Name}({{key}})",
                Queryable = true,
                HttpVerb = "GET",
                ReturnType = type,
                Parameters = new Dictionary<string, string>
                {
                    { "odata:key", Type.GetType(type.ServerType).GetIdProperty()?.GetType().FullName }
                }
            },
            new()
            {
                Name = "Get All",
                Url = $"{type.Category}/{type.Name}",
                Queryable = true,
                HttpVerb = "GET",
                ReturnType = type
            },
            new()
            {
                Name = "Delete",
                Url = $"{type.Category}/{type.Name}({{key}})",
                HttpVerb = "DELETE"
            },
        ];
    }
}