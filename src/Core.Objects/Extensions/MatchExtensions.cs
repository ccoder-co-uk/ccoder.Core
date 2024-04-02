using System.Text.RegularExpressions;

namespace Core.Objects.Extensions;

public static class MatchExtensions
{
    public static string TagName(this Match source) => 
        source.Value.Split("[".ToCharArray())[2].Replace("]", "").ToLower();

    public static (string tagName, Dictionary<string, string> attributes) TagNameAndAttributes(this Match source)
    {
        var tagName = "";
        var attributes = new Dictionary<string, string>();

        Regex tagRegex = new Regex(@"\[([^\[\]]+)\]");
        Match tagMatch = tagRegex.Match(source.Value);

        if (tagMatch.Success && tagMatch.Groups.Count > 1)
        {
            tagName = tagMatch.Groups[1].Value.ToLower();

            // Extract attributes
            Regex attrRegex = new Regex(@"(\w+)=([^|]+)");
            MatchCollection attrMatches = attrRegex.Matches(source.Value);
            foreach (Match attrMatch in attrMatches)
            {
                if (attrMatch.Groups.Count == 3)
                {
                    string attributeName = attrMatch.Groups[1].Value.ToLower();
                    string attributeValue = attrMatch.Groups[2].Value;
                    attributes[attributeName] = attributeValue;
                }
            }
        }

        return (tagName, attributes);
    }
}