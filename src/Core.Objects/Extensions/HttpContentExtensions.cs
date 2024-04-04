using System.Net.Http;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content) => Data.ParseJson<T>(await content.ReadAsStringAsync());
    }
}