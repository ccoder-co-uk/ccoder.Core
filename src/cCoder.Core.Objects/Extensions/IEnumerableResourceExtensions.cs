using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Objects.Extensions;

public static class IEnumerableResourceExtensions
{
    public static Resource WithName(this IEnumerable<Resource> resourceSection, string name)
        => resourceSection.FirstOrDefault(r => r.Name.ToLower() == name.ToLower());

    public static IEnumerable<Resource> SectionForCulture(this IEnumerable<Resource> potentials, string key, string culture)
    {
        List<Resource> results = new();

        potentials.Where(r => r.Key.ToLower() == key.ToLower())
            .GroupBy(r => r.Name.ToLower())
            .ForEach(resGroup => results.Add(resGroup.GetClosestCulturalMatch(culture)));

        return results.Where(r => r != null);
    }

    public static Resource ForNameAndCulture(this IEnumerable<Resource> potentials, string name, string culture)
    {
        List<Resource> results = new();

        potentials.Where(r => r.Name.ToLower() == name.ToLower())
            .GroupBy(r => r.Name.ToLower())
            .ForEach(resGroup => results.Add(resGroup.GetClosestCulturalMatch(culture)));

        return results.FirstOrDefault(r => r != null);
    }

    public static Resource GetClosestCulturalMatch(this IEnumerable<Resource> potentials, string culture)
    {
        Resource result = null;
        List<string> cultureParts = culture.ToLower().Split('-').ToList();
        int take = cultureParts.Count;
        string resultCulture = "";

        // scan the cultural heirarchy in the code
        while (result == null && resultCulture != null)
        {
            resultCulture = string.Join("-", cultureParts.Take(take));
            result = potentials?.FirstOrDefault(r => r.Culture?.ToLowerInvariant() == resultCulture?.ToLowerInvariant());
            take--;
            if (take == 0)
            {
                resultCulture = null;
            }
        }

        if (result == null)
        {
            result = potentials?.FirstOrDefault(r => r.Culture?.ToLowerInvariant() == string.Empty || r.Culture == null);
        }

        return result;
    }

    public static Resource ForKeyAndCulture(this IEnumerable<Resource> potentials, string cacheKey, string culture)
    {
        string[] keyAndName = cacheKey.Split('|');
        string
            key = keyAndName.Length > 1 ? keyAndName.First().ToLowerInvariant() : "default",
            name = keyAndName.Last().ToLowerInvariant();

        return potentials.Where(r => r.Key.ToLowerInvariant() == key && r.Name.ToLowerInvariant() == name).GetClosestCulturalMatch(culture);
    }
}