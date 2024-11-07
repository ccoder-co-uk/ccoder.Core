using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.Packaging.Importers;

public class FlowDefinitionImporter : CoreImporter<FlowDefinition>
{
    private readonly ILogger log;

    public FlowDefinitionImporter(
        IFlowDefinitionService service,
        ILogger<FlowDefinitionImporter> log)
            : base(service, "Core/FlowDefinition")
    { 
        this.log = log;
    }

    public override async Task Import(int appId, PackageItem item)
    {
        //
        //ProcessName property on the dynamic export...
        dynamic[] dynamicSet = item.Data.StartsWith('{') ? [item.Unpack<dynamic>()] : item.Unpack<dynamic[]>();
        FlowDefinition[] items = item.Data.StartsWith('{') ? [item.Unpack<FlowDefinition>()] : item.Unpack<FlowDefinition[]>();

        string[] names = items.Select(l => l.Name.ToLower()).ToArray();

        var dbVersions = Service.GetAll(false)
            .AsQueryable()
            .Where(c => c.AppId == appId && names.Contains(c.Name.ToLower()))
            .Select(l => new { l.Id, l.Name, ProcessName = l.App.Name })
            .ToArray();

        log.LogDebug($"Existing Flow Definition Items:\n{dbVersions.ToJsonForOdata()}");

        for (int i = 0; i < items.Length; i++)
        {
            FlowDefinition flow = items[i];
            dynamic dynamicFlow = dynamicSet[i];
            string processName = (string)dynamicFlow.ProcessName;
            var dbFlow = dbVersions.FirstOrDefault(j => dynamicSet[i].ProcessName == j.ProcessName && j.Name.ToLower() == flow.Name.ToLower());
            flow.AppId = appId;
            flow.Id = dbFlow?.Id ?? Guid.Empty;
        }

        log.LogDebug($"Expectation:\n{items.Select(i => new { i.Id, i.Name, AddOrUpdate = i.Id == Guid.Empty ? "Add" : "Update" }).ToJsonForOdata()}");

        _ = await Service.AddOrUpdate(items);
    }
}