using cCoder.Core.Objects.Dtos.Workflow;
using System;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities
{
    public abstract class LogActivity : Activity
    {

        public string Message { get; set; }

        public override Task Execute()
        {
            if (Message != null)
            {
                string level = GetType().Name.Replace("Activity", "");
                Log((WorkflowLogLevel)Enum.Parse(typeof(WorkflowLogLevel), level), Message);
            }
            return base.Execute();
        }
    }

    public class ErrorActivity : LogActivity { }

    public class WarningActivity : LogActivity { }

    public class InfoActivity : LogActivity { }

    public class DebugActivity : LogActivity { }
}
