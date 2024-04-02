using Core.Objects.Entities.CMS;
using Core.Objects.Extensions;
using System.Collections.Generic;

namespace Core.Objects.Dtos.Metadata
{
    public class PropertyContainer
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ServerType { get; set; }
        public string ServerTypeName { get; set; }
        public string Template { get; set; }
        public string DisplayName { get; set; }
        public string ShortDisplayName { get; set; }
        public string Description { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsValueType { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSystemManaged { get; set; }

        public PropertyContainer Resource(string keyContext, string culture, IEnumerable<Resource> resources)
        {
            Resource resource = resources.ForKeyAndCulture($"{keyContext}.{Name}", culture);
            return new()
            {
                Name = Name,
                Type = Type,
                ServerType = ServerType,
                ServerTypeName = ServerTypeName,
                Template = Template,
                DisplayName = resource?.DisplayName ?? DisplayName,
                ShortDisplayName = resource?.ShortDisplayName ?? ShortDisplayName,
                Description = resource?.Description ?? Description,
                IsGeneric = IsGeneric,
                IsValueType = IsValueType,
                IsReadOnly = IsReadOnly,
                IsRequired = IsRequired,
                IsSystemManaged = IsSystemManaged
            };
        }
    }
}