using System.Net;
using System.Text;
using cCoder.AppSecurity.Brokers;
using cCoder.Core.Api.OData.Responses;
using cCoder.Data;
using cCoder.Data.Models;
using Microsoft.AspNetCore.Mvc;
using ContentManagementCommonObjectCache = cCoder.ContentManagement.Exposures.Caching.ICommonObjectCache;
using ContentManagementMetadataCache = cCoder.ContentManagement.Exposures.Caching.IMetadataCache;


namespace Web.Controllers.Api
{
    [Route("Api")]
    public class ApiRootController : Controller
    {
        protected ContentManagementCommonObjectCache CommonCache { get; }
        protected ContentManagementMetadataCache MetadataCache { get; }
        protected Config Config { get; }
        protected IAuthorizationBroker AuthorizationBroker { get; }
        protected IReadOnlyList<ApiInfo> ApiContexts { get; }

        public ApiRootController(
            Config config,
            IAuthorizationBroker authorizationBroker,
            IEnumerable<ApiInfo> apiContexts,
            ContentManagementCommonObjectCache commonObjectCache,
            ContentManagementMetadataCache metadataCache
        )
        {
            CommonCache = commonObjectCache;
            MetadataCache = metadataCache;
            Config = config;
            AuthorizationBroker = authorizationBroker;
            ApiContexts = apiContexts
                .Where(context => string.Equals(context.Kind, "Context", StringComparison.OrdinalIgnoreCase))
                .OrderBy(context => context.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        [HttpGet()]
        public IActionResult Get()
        {
            var result = new
            {
                value = ApiContexts
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using StreamReader reader = new(Request.Body);
            string response = await reader.ReadToEndAsync();
            return new RawResult(response);
        }

        [HttpPut]
        public Task<IActionResult> Put() => Post();

        [HttpGet("Time")]
        public IActionResult Time() => Ok(new { DateTimeOffset.UtcNow });

        [HttpPost("ExecuteScript")]
        public async Task<IActionResult> ExecuteScript()
        {
            AuthorizationBroker.Authorize((int?)null, "script_execute");

            using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            {
                BaseAddress = new Uri(Config.Services["Workflow"]),
                Timeout = TimeSpan.FromMinutes(10)
            };

            string script = await new StreamReader(Request.Body).ReadToEndAsync();
            HttpResponseMessage response = await api.PostAsync("ExecuteScript", new StringContent(script, Encoding.UTF8, "text/plain"));
            return Ok(await response.Content.ReadAsStringAsync());
        }

        [HttpGet("GetMetadata")]
        public IActionResult GetMetadata(string culture = "")
            => Content(MetadataCache.GetAll(culture), "application/json");

        [HttpGet("RefreshCache")]
        public IActionResult RebuildCache()
        {
            CommonCache.Refresh();
            MetadataCache.Rebuild();
            return Ok();
        }
    }
}






