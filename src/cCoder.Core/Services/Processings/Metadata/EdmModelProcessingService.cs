// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Core.Dependencies.Metadata;
using cCoder.Data.Extensions;
using Microsoft.OData.Edm;
using cCoder.Core.Services.Foundations.Metadata;


namespace cCoder.Core.Services.Processings.Metadata;

internal sealed partial class EdmModelProcessingService(
    IEdmModelService edmModelService)
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

    private IEnumerable<ExtendedMetadataContainer> BuildMetadata(
        IEdmModel model,
        string contextName
    )
    {
        List<ExtendedMetadataContainer> types = [];

        EdmModelDetails edmModelDetails = edmModelService.RetrieveEdmModelDetails(
            edmModelDetails: new EdmModelDetails { Model = model });

        foreach (EdmModelType edmType in edmModelDetails.Types)
        {
            types.Add(
                item: BuildExtendedMetadataForType(
                    model: model,
                    context: contextName,
                    type: edmType.ClrType,
                    hasEndpoint: edmType.HasEndpoint));
        }

        return types.DistinctBy(keySelector: t => t.ServerTypeName);
    }

    private ExtendedMetadataContainer BuildExtendedMetadataForType(
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

        EdmModelOperations edmOperations = edmModelService.RetrieveEdmModelDetails(
            edmModelDetails: new EdmModelDetails
            {
                Model = model,
                Type = type
            }).Operations;

        if (edmOperations.HasEntitySet)
        {
            IEnumerable<OperationContainer> customOperations = edmOperations.Operations
                .Select(selector: operation => new OperationContainer
                {
                    Name = operation.Name,
                    Url = $"{result.Category}/{type.Name}/{operation.Name}()",
                    Queryable = operation.IsFunction,
                    HttpVerb = operation.IsFunction ? "GET" : "POST",
                    ReturnType = BuildMetaFor(
                        typeName: operation.ReturnTypeName,
                        isCollection: operation.ReturnIsCollection),
                    Parameters = operation.Parameters,
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

    private static MetadataContainer BuildMetaFor(
        string typeName,
        bool isCollection)
    {
        if (isCollection && !string.IsNullOrWhiteSpace(value: typeName))
        {
            Type cSharpType = Type.GetType(typeName: typeName, throwOnError: false);

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