using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Objects.Dtos.Metadata;

public class ExtendedMetadataContainer : MetadataContainer
{
    public IEnumerable<OperationContainer> Operations { get; set; }

    public ExtendedMetadataContainer() : base() { }

    public ExtendedMetadataContainer(Type type, bool isEntity = false, bool hasEndpoint = false) 
        : base(type, isEntity, hasEndpoint) { }

    public new ExtendedMetadataContainer Resource(string setName, string culture, IEnumerable<Resource> resources)
    {
        string cacheKey = $"{setName}|{ServerTypeName.Split('.').Last()}";
        Resource resource = resources.ForKeyAndCulture(cacheKey, culture);

        return new()
        {
            Type = Type,
            ServerTypeName = ServerTypeName,
            ServerType = ServerType,
            IsValueType = IsValueType,
            IsEntity = IsEntity,
            IsJoinEntity = IsJoinEntity,
            HasEndpoint = HasEndpoint,
            IsSystemManaged = IsSystemManaged,
            Category = Category,
            Name = Name,
            DisplayName = resource?.DisplayName ?? DisplayName,
            Description = resource?.Description ?? Description,
            Properties = Properties.Select(p => p.Resource(cacheKey, culture, resources)).ToArray(),
            Methods = Methods, 
            Operations = Operations,
        };
    }
}