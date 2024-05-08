using cCoder.Core.Objects.Entities.CMS;
using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Dtos.Metadata;

public class MetadataContainerSet
{
    [Required]
    public string Name { get; set; }

    public string UriBase { get; set; }

    public MetadataContainer[] Types { get; set; }

    public MetadataContainerSet Resource(string culture, IEnumerable<Resource> resources)
        => new()
        {
            Name = Name,
            UriBase = UriBase,
            Types = Types.Select(t => t.Resource(Name, culture, resources)).ToArray()
        };
}