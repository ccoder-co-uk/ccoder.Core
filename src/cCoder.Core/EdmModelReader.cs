// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core;

internal static class EdmModelReader
{
    private static readonly Func<IEdmModel, IEdmSchemaType, Type> ClrTypeResolver =
        (model, edmType) => model.GetAnnotationValue<ClrTypeAnnotation>(
            element: edmType)?.ClrType;

    internal static IReadOnlyCollection<(Type ClrType, bool HasEndpoint)> GetTypes(
        IEdmModel model)
    {
        List<(Type ClrType, bool HasEndpoint)> types = [];

        foreach (IEdmEntitySet entitySet in model.EntityContainer.EntitySets())
        {
            Type clrType = GetClrType(model: model, edmType: entitySet.EntityType);

            if (clrType is not null)
            {
                types.Add(item: (clrType, true));
            }
        }

        foreach (IEdmSchemaType schemaType in model.SchemaElements.OfType<IEdmSchemaType>())
        {
            if (schemaType is not IEdmComplexType && schemaType is not IEdmEntityType)
            {
                continue;
            }

            Type clrType = GetClrType(model: model, edmType: schemaType);

            if (clrType is not null)
            {
                bool hasEndpoint = model.EntityContainer.FindEntitySet(
                    setName: clrType.Name) is not null;

                types.Add(item: (clrType, hasEndpoint));
            }
        }

        return types;
    }

    internal static (
        bool HasEntitySet,
        IReadOnlyCollection<(
            string Name,
            bool IsFunction,
            bool ReturnIsCollection,
            string ReturnTypeName,
            IDictionary<string, string> Parameters)> Operations) GetOperations(
        IEdmModel model,
        Type type)
    {
        IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(
            setName: type.Name);

        if (entitySet is null)
        {
            return (false, []);
        }

        (
            string Name,
            bool IsFunction,
            bool ReturnIsCollection,
            string ReturnTypeName,
            IDictionary<string, string> Parameters)[] operations =
        [
            .. model.FindDeclaredBoundOperations(bindingType: entitySet.Type)
                .Select(selector: operation =>
                (
                    operation.Name,
                    operation.IsFunction(),
                    operation.GetReturn()?.Type?.Definition?.TypeKind
                        == EdmTypeKind.Collection,
                    operation.GetReturn()?.Type?.Definition?.FullTypeName(),
                    operation.Parameters?
                        .Where(predicate: parameter => parameter.Name != "bindingParameter")
                        .ToDictionary(
                            keySelector: parameter => parameter.Name,
                            elementSelector: parameter => parameter.Type.FullName())
                        ?? new Dictionary<string, string>()
                ))
        ];

        return (true, operations);
    }

    private static Type GetClrType(IEdmModel model, IEdmSchemaType edmType) =>
        ClrTypeResolver(model, edmType);
}