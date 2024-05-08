using cCoder.Core.Api.OData.Responses;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security;
using System.Text;

namespace Web.Controllers.Api;

[Route("Api")]
public class ApiRootController : Controller
{
    protected ICommonObjectCache CommonCache { get; }
    protected IMetadataCache MetadataCache { get; }

    protected Config Config { get; }

    protected User CoreUser { get; }

    public ApiRootController(Config config, IAppService appService, ICommonObjectCache commonObjectCache, IMetadataCache metadataCache)
    {
        CommonCache = commonObjectCache;
        MetadataCache = metadataCache;
        Config = config;
        CoreUser = appService.User;
    }

    /// <summary>
    /// Test Method to Ensure the basic system is up
    /// </summary>
    /// <returns>context listing</returns>
    [HttpGet()]
    public IActionResult Get()
    {
        var result = new
        {
            value = new[] {
                new ApiInfo { Kind = "Context", Name = "Core", Url = "Core" },
                new ApiInfo { Kind = "Context", Name = "Members", Url = "Members" },
                new ApiInfo { Kind = "Context", Name = "B2B", Url = "B2B" }
            }
        };

        return Ok(result);
    }

    /// <summary>
    /// Test call for remote callers to confirm the API is up
    /// </summary>
    /// <returns>What was sent</returns>
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        using StreamReader reader = new(Request.Body);
        string response = await reader.ReadToEndAsync();
        return new RawResult(response);
    }

    /// <summary>
    /// Test call for remote callers to confirm the API is up
    /// </summary>
    /// <returns>What was sent</returns>
    [HttpPut]
    public Task<IActionResult> Put() => Post();

    /// <summary>
    /// Returns the current UTC server time 
    /// </summary>
    /// <returns></returns>
    [HttpGet("Time")]
    public IActionResult Time() => Ok(new { DateTimeOffset.UtcNow });

    /// <summary>
    /// Execute the given code 
    /// </summary>
    /// <returns>result as raw string</returns>
    [HttpPost("ExecuteScript")]
    public async Task<IActionResult> ExecuteScript()
    {
        if (!CoreUser.Can(null, "script_execute"))
            throw new SecurityException("Access Denied!");

        using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        {
            BaseAddress = new Uri(Config.Services["Workflow"]),
            Timeout = TimeSpan.FromMinutes(10)
        };

        string script = await new StreamReader(Request.Body).ReadToEndAsync();
        HttpResponseMessage response = await api.PostAsync("ExecuteScript", new StringContent(script, Encoding.UTF8, "text/plain"));
        return Ok(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Lists all metadata that makes up the API Layer type set
    /// </summary>
    /// <returns>Metadata contrainer array contianing meta type info</returns>
    [HttpGet("GetMetadata")]
    public IActionResult GetMetadata(string culture = "")
        => Content(MetadataCache.GetAll(culture), "application/json");

    /// <summary>
    /// Rebuilds the common cache from the raw data
    /// </summary>
    /// <returns></returns>
    [HttpGet("RefreshCache")]
    public IActionResult RebuildCache()
    {
        CommonCache.Refresh();
        MetadataCache.Rebuild();
        return Ok();
    }

    [HttpPost("UpgradeSystem")]
    public async Task<IActionResult> UpgradeSystem()
    {
        using HttpClient schedulerClient = new() { BaseAddress = new Uri(Config.Services["Scheduler"]) };
        return Ok(await (await schedulerClient.PostAsync("Migrate", null)).Content.ReadAsStringAsync());
    }
}