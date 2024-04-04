using System.Xml.Linq;

namespace cCoder.Core.Objects.Extensions
{
    public static class XElementExtensions
    {
        public static string Att(this XElement source, string name) => source.Attribute(name)?.Value;

        public static string Reference(this XElement e, string name, string source) => e.Att(name) != null && e.Att(name).Contains('|') ? e.Att(name) : $"{source}|{e.Att(name)}";

        public static decimal AttDecimal(this XElement source, string name)
        {
            string rawValue = source.Attribute(name)?.Value;
            return rawValue.IsNullOrEmpty() ? 0.00M : decimal.Parse(rawValue);
        }
    }
}
