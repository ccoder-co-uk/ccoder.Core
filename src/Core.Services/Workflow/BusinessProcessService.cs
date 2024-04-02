using Core.Objects;
using Core.Objects.Entities.Workflow;

namespace Core.Services.Workflow
{
    public class BusinessProcessService : CoreService<BusinessProcess>
    {
        public BusinessProcessService(ICoreDataContext db) : base(db) { }
    }
}