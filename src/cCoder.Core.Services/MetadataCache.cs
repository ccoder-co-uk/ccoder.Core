using cCoder.Core.Data;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Extensions;

namespace cCoder.Core.Services;

public class MetadataCache : Cache<MetadataContainerSet>, IMetadataCache
{
    private readonly IDictionary<string, IDictionary<string, string>> metaSerialized;
    private readonly IEnumerable<MetadataContainerSet> typeSet;
    private readonly ICommonObjectCache resourceCache;

    public MetadataCache(IEnumerable<MetadataContainerSet> typeSet, ICommonObjectCache resourceCache) : base()
    {
        metaSerialized = new Dictionary<string, IDictionary<string, string>>();
        this.typeSet = typeSet;
        this.resourceCache = resourceCache;
        Rebuild();
    }

    public string GetAll(string culture = "") => "[" + string.Join(',', typeSet.Select(c => metaSerialized[culture][c.Name.ToLower()]).ToArray()) + "]";


    public void Rebuild()
    {
        metaSerialized.Clear();
        Resource[] resourceSet = resourceCache.GetAll<Resource>();

        Cultures.Known.ForEach(culture =>
        {
            metaSerialized.Add(culture.Id, new Dictionary<string, string>());
            typeSet.ForEach(c =>
            {
                MetadataContainerSet resourcedContainerSet = c.Resource(culture.Id, resourceSet);
                Set(resourcedContainerSet.Name.ToLower(), resourcedContainerSet.ToJsonForOdata(), culture.Id);
                resourcedContainerSet.Types.ForEach(d => Set(c.Name.ToLower() + "/" + d.Name.ToLower(), d.ToJsonForOdata(), culture.Id));
            });
        });
    }

    public void Set(string key, string value, string culture)
    {
        if (metaSerialized[culture].ContainsKey(key))
            metaSerialized[culture][key] = value;
        else
            metaSerialized[culture].Add(new KeyValuePair<string, string>(key, value));
    }

    public string ToJson(string culture) => metaSerialized[culture].ToJsonForOdata();

    string IMetadataCache.Get(string key, string culture) => metaSerialized[culture].ContainsKey(key) ? metaSerialized[culture][key] : string.Empty;
}