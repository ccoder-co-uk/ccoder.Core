using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Workflow;

namespace cCoder.Core.Services.Workflow;

public class BusinessProcessService : CoreService<BusinessProcess>
{
    public BusinessProcessService(ICoreDataContext db) : base(db) { }
}