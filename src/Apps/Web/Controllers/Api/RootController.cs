// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Text;
using cCoder.AppSecurity.Brokers;
using cCoder.Core.Exposures.OData.Responses;
using cCoder.Data;
using cCoder.Data.Models;
using Microsoft.AspNetCore.Mvc;


namespace Web.Controllers.Api
{
    [Route("Api")]
    public class ApiRootController : Controller
    {
        protected readonly Config Config;
        protected readonly IAuthorizationBroker AuthorizationBroker;
        protected readonly IReadOnlyList<ApiInfo> ApiContexts;

        public ApiRootController(
            Config config,
            IAuthorizationBroker authorizationBroker,
            IEnumerable<ApiInfo> apiContexts
        )
        {
            Config = config;
            AuthorizationBroker = authorizationBroker;
            ApiContexts = apiContexts
                .Where(predicate: context =>
                    string.Equals(
                        a: context.Kind,
                        b: "Context",
                        comparisonType: StringComparison.OrdinalIgnoreCase))
                .OrderBy(keySelector: context => context.Name,comparer: StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        [HttpGet()]
        public IActionResult Get()
        {
            var result = new
            {
                value = ApiContexts
            };

            return Ok(value: result);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using StreamReader reader = new(Request.Body);
            string response = await reader.ReadToEndAsync();
            return new RawResult(response);
        }

        [HttpPut]
        public Task<IActionResult> Put() =>
            Post();

        [HttpGet("Time")]
        public IActionResult GetTime() =>
            Ok(value: new { DateTimeOffset.UtcNow });

        [HttpPost("ExecuteScript")]
        public async Task<IActionResult> PostExecuteScript()
        {
            AuthorizationBroker.Authorize(appId: (int?)null,privilege: "script_execute");

            using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            {
                BaseAddress = new Uri(Config.Services["Workflow"]),
                Timeout = TimeSpan.FromMinutes(minutes: 10)
            };

            string script = await new StreamReader(Request.Body).ReadToEndAsync();
            HttpResponseMessage response = await api.PostAsync(requestUri: "ExecuteScript",content: new StringContent(script, Encoding.UTF8, "text/plain"));
            return Ok(value: await response.Content.ReadAsStringAsync());
        }

    }
}