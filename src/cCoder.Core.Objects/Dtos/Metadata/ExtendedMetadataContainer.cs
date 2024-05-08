namespace cCoder.Core.Objects.Dtos.Metadata;

public class ExtendedMetadataContainer : MetadataContainer
{
    public IEnumerable<OperationContainer> Operations { get; set; }

    public ExtendedMetadataContainer() : base() { }

    public ExtendedMetadataContainer(Type type) : this(type, true, true) { }

    public ExtendedMetadataContainer(Type type, bool hasEndpoint) : this(type, true, hasEndpoint) { }

    public ExtendedMetadataContainer(Type type, bool isEntity, bool hasEndpoint) : base(type, isEntity, hasEndpoint) { }
}