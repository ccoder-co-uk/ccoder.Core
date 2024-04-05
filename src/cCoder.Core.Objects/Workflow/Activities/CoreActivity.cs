using cCoder.Core.Objects.Workflow.Activities.Api;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities
{
    public abstract class CoreActivity : ApiActivity
    {
        [Picker("Core/App")]
        public int AppId { get; set; }

        public override Task ExecuteInternal(IWorkflowContext context)
        {
            AppId = (int)context.Variables["AppId"];
            return base.ExecuteInternal(context);
        }
    }
}