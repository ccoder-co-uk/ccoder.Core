using System.Text.RegularExpressions;

namespace Core.Objects.Extensions
{
    public static class MatchExtensions
    {
        public static string TagName(this Match source) => source.Value.Split("[".ToCharArray())[2].Replace("]", "").ToLower();
    }
}