using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Entities.Workflow;
using Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace Core.Services.Workflow
{
    public class FlowDefinitionService : CoreService<FlowDefinition>, IFlowDefinitionService
    {
        private readonly Config config;
        readonly ILogger log;

        public FlowDefinitionService(Config config, ICoreDataContext db, ILogger<FlowDefinitionService> log) 
            : base(db)
        {
            this.config = config;
            this.log = log;
        }

        public async Task<Guid> Queue(Guid id, string args)
        {
            var flow = Db.GetAll<FlowDefinition>(false)
                .Include(f => f.App)
                .FirstOrDefault(f => f.Id == id);

            bool shouldRunNow = !Db.GetAll<FlowInstanceData>()
                .Any(d => d.FlowDefinitionId == id && (d.State == "Queued" || d.State == "Executing"));

            var result = flow != null
                ? await flow.QueueNewInstance(Db, Db.User, args)
                : throw new SecurityException("Access Denied!");


            // We are not awaiting this deliberately, ignore the warning.
            if (shouldRunNow)
                Task.Run(() => ExecuteNextQueuedInstance(id));

            return result;
        }

        public override async Task DeleteAsync(object id)
        {
            var flow = Db.GetAll<FlowDefinition>(true)
                .Include(fd => fd.Instances)
                .FirstOrDefault(fd => fd.Id == (Guid)id);

            if (flow != null)
            {
                await Db.DeleteAllAsync(flow.Instances);
                _ = await Db.DeleteAsync(flow);
            }
            else
                throw new SecurityException("Access Denied!");
        }

        public override Task<IEnumerable<Result<FlowDefinition>>> AddAllAsync(IEnumerable<FlowDefinition> items)
        {
            log.LogDebug($"Adding:\n{items.Select(i => new { i.Id, i.Name }).ToJsonForOdata()}");
            return base.AddAllAsync(items);
        }

        public override Task<IEnumerable<Result<FlowDefinition>>> UpdateAllAsync(IEnumerable<FlowDefinition> items)
        {
            log.LogDebug($"Updating:\n{items.Select(i => new { i.Id, i.Name }).ToJsonForOdata()}");
            return base.UpdateAllAsync(items);
        }

        async Task ExecuteNextQueuedInstance(Guid flowDefinitionId)
        {
            var scheduler = config.Services["Scheduler"];

            using HttpClient api = new(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            {
                BaseAddress = new Uri(scheduler)
            };

            api.Timeout = TimeSpan.FromMinutes(11);

            var response = await api.PostAsync("ExecuteNextFlowInstanceInQueue?flowId=" + flowDefinitionId, null);
            _ = response.EnsureSuccessStatusCode();
        }
    }
}