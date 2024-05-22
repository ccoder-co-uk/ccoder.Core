using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Extensions;
using Microsoft.OData.Edm;

namespace cCoder.Core.Api;

public static class IEdmModelExtensions
{
    public static ExtendedMetadataContainer GetExtendedMetadataForType(this IEdmModel model, string context, Type type, bool hasEndpoint = true)
    {
        ExtendedMetadataContainer result = new(type, true, hasEndpoint) { Category = context };
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

    private static IEnumerable<OperationContainer> GetBaseCRUDOperations(MetadataContainer type)
        => type.IsJoinEntity ? GetBaseCRUDOperationsForJoinEntity(type) : GetBaseCRUDOperationsForEntity(type);

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForJoinEntity(MetadataContainer type)
        => new List<OperationContainer>
        {
            new() {
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
            new() {
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
            new() { Name = "Get All", Url = $"{type.Category}/{type.Name}", Queryable = true, HttpVerb = "GET", ReturnType = type },
            new() { Name = "Delete", Url = $"{type.Category}/{type.Name}({{Left=leftKey,Right=rightKey}})", HttpVerb = "DELETE" },
        };

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForEntity(MetadataContainer type)
    {
        try
        {
            return new List<OperationContainer>
            {
                new() {
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
                new() {
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
                new() {
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
                new() { Name = "Get All", Url = $"{type.Category}/{type.Name}", Queryable = true, HttpVerb = "GET", ReturnType = type },
                new() { Name = "Delete", Url = $"{type.Category}/{type.Name}({{key}})", HttpVerb = "DELETE" },
            };
        }
        catch (NullReferenceException) { return Array.Empty<OperationContainer>(); }
    }
}