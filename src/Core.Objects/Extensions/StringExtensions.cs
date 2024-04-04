using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace cCoder.Core.Objects.Extensions
{
    public static class StringExtensions
    {
        public static string[] SplitCamelCase(this string source)
        {
            return Regex.Split(source, @"(?<!^)(?=[A-Z])");
        }

        public static string RegexReplace(this string source, string matchExpression, Func<Match, string> action)
        {
            StringBuilder builder = new(source);
            builder.RegexReplace(matchExpression, action);
            return builder.ToString();
        }

        public static void RegexReplace(this StringBuilder source, string matchExpression, Func<Match, string> action)
        {
            MatchCollection matches = Regex.Matches(source.ToString(), matchExpression, RegexOptions.CultureInvariant & RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                _ = source.Replace(m.Value, action(m));
            }
        }

        public static void RegexMatch(this StringBuilder source, string matchExpression, Action<Match> action)
        {
            MatchCollection matches = Regex.Matches(source.ToString(), matchExpression, RegexOptions.CultureInvariant & RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                action(m);
            }
        }

        public static Stream ToStream(this string source)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Parses a datetime object from the given string in the format "YYYYMMdd"
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns>resulting datetime</returns>
        public static DateTime? ToDateFromYYYYMMdd(this string source)
        {
            try
            {
                return source != null
                    ? DateTime.Parse(source.Insert(4, "/").Insert(7, "/"))
                    : null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Parses a datetime object from the given string in the format "YYYYMMdd"
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns>resulting datetime</returns>
        public static DateTime? ToDateFromddMMYYYY(this string source)
        {
            try
            {
                return source != null
                    ? DateTime.Parse(source.Insert(2, "/").Insert(5, "/"))
                    : null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Makes a string in to a valid path string by replacing invalid chars commonly used
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns>the valid path string</returns>
        public static string ReplaceIllegalPathChars(this string source)
        {
            return source?.Replace("|", "_")
                .Replace("/", "_")
                .Replace(@"\", "_")
                .Replace(":", "_");
        }

        public static bool IsNullOrEmpty(this string source) => source == null || source.Length == 0;
    }
}
