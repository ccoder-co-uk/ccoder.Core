using cCoder.Core.Objects;
using cCoder.Core.Objects.Dtos.Metadata;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Workflow.Activities;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;

namespace cCoder.Core.Api.Controllers
{
    public class FlowDefinitionController : CoreEntityODataController<FlowDefinition, Guid>
    {
        protected new IFlowDefinitionService Service => base.Service as IFlowDefinitionService;

        protected Config Config { get; set; }

        public FlowDefinitionController(IFlowDefinitionService service, ICoreAuthInfo auth, Config config, ILogger<FlowDefinitionController> log) 
            : base(service, auth, log) =>
                Config = config;

        [HttpPost]
        public async Task<IActionResult> Execute([FromRoute] Guid key)
        {
            using StreamReader reader = new(Request.Body, Encoding.UTF8);
            return Ok(await Service.Queue(key, await reader.ReadToEndAsync()));
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteScript()
        {
            if (!Service.User.Can(null, "script_execute"))
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


        [HttpGet]
        [EnableQuery(
            AllowedArithmeticOperators = AllowedArithmeticOperators.All,
            AllowedFunctions = AllowedFunctions.All,
            AllowedLogicalOperators = AllowedLogicalOperators.All,
            AllowedQueryOptions = AllowedQueryOptions.All,
            MaxAnyAllExpressionDepth = 6,
            MaxExpansionDepth = 6
        )]
        public IActionResult KnownActivityTypes()
        {
            Type baseType = typeof(Activity);
            MetadataContainerSet[] types = TypeHelper.GetWebStackAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsSubclassOf(baseType) && t != baseType))
                .GroupBy(t => t.BaseType.Name.Split('`')[0])
                .Select(g => new MetadataContainerSet
                {
                    Name = g.Key,
                    Types = g.Where(t => !t.IsAbstract)
                        .Select(t =>
                        {
                            Type type = t.IsGenericType
                                ? t.MakeGenericType(t.GetTypeInfo().GenericTypeParameters.Select(i => typeof(object)).ToArray())
                                : t;

                            return new MetadataContainer(type);
                        }).ToArray()
                })
                .ToArray();

            return Ok(types);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult KnownSystemTypes()
        {
            MetadataContainerSet[] systemTypes = new[] {
                new MetadataContainerSet
                {
                    Name = "System",
                    Types = new [] 
                    {
                        new MetadataContainer(typeof(int)),
                        new MetadataContainer(typeof(string)),
                        new MetadataContainer(typeof(decimal)),
                        new MetadataContainer(typeof(float)),
                        new MetadataContainer(typeof(bool)),
                        new MetadataContainer(typeof(DateTime)),
                        new MetadataContainer(typeof(DateTimeOffset)),
                        new MetadataContainer(typeof(TimeSpan)),
                        new MetadataContainer(typeof(object)),
                        new MetadataContainer(typeof(System.Dynamic.ExpandoObject)),
                        new MetadataContainer(typeof(IEnumerable<object>))
                    }
                }
            };

            return Ok(systemTypes);
        }
    }
}