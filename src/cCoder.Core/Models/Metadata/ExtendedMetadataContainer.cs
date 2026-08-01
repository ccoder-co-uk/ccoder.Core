// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Metadata;

public class ExtendedMetadataContainer : MetadataContainer
{
    public IEnumerable<OperationContainer> Operations { get; set; }

    public ExtendedMetadataContainer()
    {
    }

    public ExtendedMetadataContainer(Type type)
        : base(type)
    {
    }

    public ExtendedMetadataContainer(
        Type type,
        bool isEntity,
        bool hasEndpoint)
        : base(type, isEntity, hasEndpoint)
    {
    }
}