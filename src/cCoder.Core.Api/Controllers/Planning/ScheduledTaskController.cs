using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace cCoder.Core.Api.Controllers
{
    public class ScheduledTaskController : CoreEntityODataController<ScheduledTask, int>
    {
        protected new IScheduledTaskService Service => 
            base.Service as IScheduledTaskService;

        public ScheduledTaskController(IScheduledTaskService service, ICoreAuthInfo auth, ILogger<ScheduledTaskController> log) 
            : base(service, auth, log) { }

        [HttpPost]
        public async Task<IActionResult> Execute([FromRoute] int key, bool incrementNextExecution = true)
        {
            await Service.Execute(key, incrementNextExecution);
            return Ok();
        }
    }
}