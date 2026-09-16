// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Metadata;
using Microsoft.OData.Edm;

namespace cCoder.Core.Brokers.Metadata;

internal sealed class EdmModelBroker : IEdmModelBroker
{
    public IReadOnlyCollection<EdmModelType> RetrieveTypes(IEdmModel model) =>
        [
            .. EdmModelReader.GetTypes(model: model)
                .Select(selector: type => new EdmModelType
                {
                    ClrType = type.ClrType,
                    HasEndpoint = type.HasEndpoint,
                }),
        ];

    public EdmModelOperations RetrieveOperations(
        IEdmModel model,
        Type type)
    {
        (bool HasEntitySet, IReadOnlyCollection<(string Name, bool IsFunction, bool ReturnIsCollection, string ReturnTypeName, IDictionary<string, string> Parameters)> Operations) details =
            EdmModelReader.GetOperations(
                model: model,
                type: type);

        return new EdmModelOperations
        {
            HasEntitySet = details.HasEntitySet,
            Operations =
            [
                .. details.Operations.Select(selector: operation => new EdmModelOperation
                {
                    Name = operation.Name,
                    IsFunction = operation.IsFunction,
                    ReturnIsCollection = operation.ReturnIsCollection,
                    ReturnTypeName = operation.ReturnTypeName,
                    Parameters = operation.Parameters,
                }),
            ],
        };
    }
}