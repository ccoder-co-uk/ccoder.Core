// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using cCoder.Data.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Dependencies.Metadata;

internal static class EdmModelDependency
{
    internal static EdmModelDetails RetrieveEdmModelDetails(
        EdmModelDetails edmModelDetails)
    {
        if (edmModelDetails.Type is null)
        {
            edmModelDetails.Types = GetTypes(model: edmModelDetails.Model);
        }
        else
        {
            edmModelDetails.Operations = GetOperations(
                model: edmModelDetails.Model,
                type: edmModelDetails.Type);
        }

        return edmModelDetails;
    }

    private static IReadOnlyCollection<EdmModelType> GetTypes(object model)
    {
        IEdmModel edmModel = (IEdmModel)model;
        List<EdmModelType> types = [];

        foreach (IEdmEntitySet entitySet in edmModel.EntityContainer.EntitySets())
        {
            Type clrType = GetClrType(model: edmModel, edmType: entitySet.EntityType);

            if (clrType is not null)
            {
                types.Add(item: new EdmModelType
                {
                    ClrType = clrType,
                    HasEndpoint = true
                });
            }
        }

        foreach (IEdmSchemaType schemaType in edmModel.SchemaElements.OfType<IEdmSchemaType>())
        {
            if (schemaType is not IEdmComplexType && schemaType is not IEdmEntityType)
            {
                continue;
            }

            Type clrType = GetClrType(model: edmModel, edmType: schemaType);

            if (clrType is not null)
            {
                bool hasEndpoint = edmModel.EntityContainer.FindEntitySet(setName: clrType.Name) is not null;

                types.Add(item: new EdmModelType
                {
                    ClrType = clrType,
                    HasEndpoint = hasEndpoint
                });
            }
        }

        return types;
    }

    private static EdmModelOperations GetOperations(object model, Type type)
    {
        IEdmModel edmModel = (IEdmModel)model;
        IEdmEntitySet entitySet = edmModel.EntityContainer.FindEntitySet(setName: type.Name);

        if (entitySet is null)
        {
            return new EdmModelOperations
            {
                HasEntitySet = false,
                Operations = []
            };
        }

        EdmModelOperation[] operations =
        [
            .. edmModel.FindDeclaredBoundOperations(bindingType: entitySet.Type)
                .Select(selector: operation => new EdmModelOperation
                {
                    Name = operation.Name,
                    IsFunction = operation.IsFunction(),
                    ReturnIsCollection =
                        operation.GetReturn()?.Type?.Definition?.TypeKind
                            == EdmTypeKind.Collection,
                    ReturnTypeName = operation.GetReturn()?.Type?.Definition?.FullTypeName(),
                    Parameters = operation.Parameters?
                        .Where(predicate: parameter => parameter.Name != "bindingParameter")
                        .ToDictionary(
                            keySelector: parameter => parameter.Name,
                            elementSelector: parameter => parameter.Type.FullName())
                        ?? new Dictionary<string, string>()
                })
        ];

        return new EdmModelOperations
        {
            HasEntitySet = true,
            Operations = operations
        };
    }

    private static Type GetClrType(IEdmModel model, IEdmSchemaType edmType) =>
        model.GetAnnotationValue<ClrTypeAnnotation>(element: edmType)?.ClrType;
}
