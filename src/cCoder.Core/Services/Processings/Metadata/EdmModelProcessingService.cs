// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Core.Dependencies.Metadata;
using cCoder.Data.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;


namespace cCoder.Core.Services.Processings.Metadata;

internal sealed partial class EdmModelProcessingService
    : IEdmModelProcessingService
{
    public IEnumerable<ExtendedMetadataContainer> GetEdmModelMetadata(
        IEdmModel model,
        string contextName) =>
        TryCatch(operation: () =>
        {
            ValidateEdmModelMetadataOnGet(
                model: model,
                contextName: contextName);

            return BuildMetadata(
                model: model,
                contextName: contextName);
        });

    public ExtendedMetadataContainer GetExtendedMetadataContainer(
        IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true) =>
        TryCatch(operation: () =>
        {
            ValidateExtendedMetadataContainerOnGet(
                model: model,
                context: context,
                type: type,
                hasEndpoint: hasEndpoint);

            return BuildExtendedMetadataForType(
                model: model,
                context: context,
                type: type,
                hasEndpoint: hasEndpoint);
        });

    private static IEnumerable<ExtendedMetadataContainer> BuildMetadata(
        IEdmModel model,
        string contextName
    )
    {
        List<ExtendedMetadataContainer> types = [];

        foreach (var entitySet in model.EntityContainer.EntitySets())
        {
            var clr = GetClrType(model: model, edmType: entitySet.EntityType);

            if (clr != null)
            {
                types.Add(
                    item: BuildExtendedMetadataForType(
                        model: model,
                        context: contextName,
                        type: clr,
                        hasEndpoint: true));
            }
        }

        foreach (var schemaType in model.SchemaElements.OfType<IEdmSchemaType>())
        {
            if (schemaType is IEdmComplexType || schemaType is IEdmEntityType)
            {
                var clr = GetClrType(model: model, edmType: schemaType);

                if (clr != null)
                {
                    bool hasEndpoint = model.EntityContainer.FindEntitySet(setName: clr.Name) != null;

                    types.Add(
                        item: BuildExtendedMetadataForType(
                            model: model,
                            context: contextName,
                            type: clr,
                            hasEndpoint: hasEndpoint));
                }
            }
        }

        return types.DistinctBy(keySelector: t => t.ServerTypeName);
    }

    private static ExtendedMetadataContainer BuildExtendedMetadataForType(
        IEdmModel model,
        string context,
        Type type,
        bool hasEndpoint = true
    )
    {
        ExtendedMetadataContainer result =
            MetadataContainerDependency.CreateExtendedMetadataContainer(
                type: type,
                isEntity: true,
                hasEndpoint: hasEndpoint);

        result.Category = context;

        IEdmEntitySet set = model.EntityContainer.FindEntitySet(setName: type.Name);

        if (set != null)
        {
            IEnumerable<OperationContainer> customOperations = model
                .FindDeclaredBoundOperations(bindingType: set.Type)
                .Select(selector: o => new OperationContainer
                {
                    Name = o.Name,
                    Url = $"{result.Category}/{type.Name}/{o.Name}()",
                    Queryable = o.IsFunction(),
                    HttpVerb = o.IsFunction() ? "GET" : "POST",
                    ReturnType = BuildMetaFor(definition: o.GetReturn()?.Type?.Definition),
                    Parameters = o
                        .Parameters?.Where(predicate: p => p.Name != "bindingParameter")
                        .Select(selector: p => new { k = p.Name, v = p.Type.FullName() })
                        .ToDictionary(keySelector: i => i.k, elementSelector: i => i.v),
                });

            result.Operations =
            [
                .. GetBaseCRUDOperations(type: result)
                    .Union(second: customOperations)
            ];
        }
        else
        {
            result.HasEndpoint = false;
        }

        return result;
    }

    static Type GetClrType(IEdmModel model, IEdmSchemaType edmType) =>
        model.GetAnnotationValue<ClrTypeAnnotation>(element: edmType)?.ClrType;

    private static MetadataContainer BuildMetaFor(IEdmType definition)
    {
        if (definition != null && definition.TypeKind == EdmTypeKind.Collection)
        {
            Type cSharpType = Type.GetType(typeName: definition.FullTypeName(), throwOnError: false);

            if (cSharpType != null)
            {
                return MetadataContainerDependency.CreateMetadataContainer(
                    type: cSharpType,
                    isEntity: true,
                    hasEndpoint: true);
            }
        }

        return null;
    }

    private static IEnumerable<OperationContainer> GetBaseCRUDOperations(MetadataContainer type) =>
        type.IsJoinEntity
            ? GetBaseCRUDOperationsForJoinEntity(type: type)
            : GetBaseCRUDOperationsForEntity(type: type);

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForJoinEntity(
        MetadataContainer type
    ) =>
        [
            new()
            {
                Name = "Add",
                Url = $"{type.Category}/{type.Name}",
                Queryable = true,
                HttpVerb = "POST",
                ReturnType = type,
                Parameters = new Dictionary<string, string> { { "body:entity", type.ServerType } },
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
                    {
                        "odata:key",
                        Type.GetType(typeName: type.ServerType)
                            .GetIdProperty()
                            .GetType().FullName
                    },
                },
            },
            new()
            {
                Name = "Get All",
                Url = $"{type.Category}/{type.Name}",
                Queryable = true,
                HttpVerb = "GET",
                ReturnType = type,
            },
            new()
            {
                Name = "Delete",
                Url = $"{type.Category}/{type.Name}({{Left=leftKey,Right=rightKey}})",
                HttpVerb = "DELETE",
            },
        ];

    private static IEnumerable<OperationContainer> GetBaseCRUDOperationsForEntity(
        MetadataContainer type
    )
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
                Parameters = new Dictionary<string, string> { { "body:entity", type.ServerType } },
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
                    {
                        "odata:key",
                        Type.GetType(typeName: type.ServerType)
                            .GetIdProperty()?.GetType().FullName
                    },
                    { "body:entity", type.ServerType },
                },
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
                    {
                        "odata:key",
                        Type.GetType(typeName: type.ServerType)
                            .GetIdProperty()?.GetType().FullName
                    },
                },
            },
            new()
            {
                Name = "Get All",
                Url = $"{type.Category}/{type.Name}",
                Queryable = true,
                HttpVerb = "GET",
                ReturnType = type,
            },
            new()
            {
                Name = "Delete",
                Url = $"{type.Category}/{type.Name}({{key}})",
                HttpVerb = "DELETE",
            },
        ];
    }
}