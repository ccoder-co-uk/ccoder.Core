using System.Security;
using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Services.Workflow;

public class FlowInstanceDataService(ICoreDataContext db, Config config) 
    : CoreService<FlowInstanceData>(db)
{
    public override async Task<FlowInstanceData> UpdateAsync(FlowInstanceData entity)
    {
        FlowInstanceData dbVersion = Db.GetAll<FlowInstanceData>()
            .Include(fd => fd.FlowDefinition)
            .FirstOrDefault(f => f.Id == entity.Id);

        dbVersion.UpdateFrom(entity);

        FlowInstanceData result = User.Can(dbVersion.FlowDefinition.AppId, "flowinstancedata_update")
            ? await Db.UpdateAsync(dbVersion)
            : throw new SecurityException("Access Denied!");

        return result;
    }
}