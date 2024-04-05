using System.IO;

namespace cCoder.Core.Objects.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ToArray(this Stream input)
        {
            using MemoryStream ms = new();
            input.CopyToAsync(ms).Wait();
            return ms.ToArray();
        }
    }
}