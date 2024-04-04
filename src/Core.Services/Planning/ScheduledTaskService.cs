using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Planning;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.CMS
{
    public class ScheduledTaskService : CoreService<ScheduledTask>, IScheduledTaskService
    {
        public ScheduledTaskService(ICoreDataContext db) : base(db) { }

        // executes a task right now as the user that made the call
        public async Task Execute(int id, bool incrementNextExecution = true)
        {
            ScheduledTask task = Db.GetAll<ScheduledTask>()
                .Include(t => t.Flow)
                    .ThenInclude(f => f.App)
                .Include(t => t.ExecuteAsUser)
                    .ThenInclude(u => u.Roles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefault(t => t.Id == id);

            if (task != null && User.IsAdminOfApp(task.AppId))
                await task.Execute(Db, incrementNextExecution);
            else
                throw new SecurityException("Access Denied!");
        }

        public override Task<ScheduledTask> AddAsync(ScheduledTask entity) =>
            SecurityCheckTask(entity)
                ? base.AddAsync(entity)
                : throw new SecurityException("Access Denied!");

        public override Task<ScheduledTask> UpdateAsync(ScheduledTask entity) =>
            SecurityCheckTask(entity)
                ? base.UpdateAsync(entity)
                : throw new SecurityException("Access Denied!");

        // checks to confirm task isn't bleeding beyond app scope and that an app admin is the one setting this up
        private bool SecurityCheckTask(ScheduledTask task)
        {
            bool userIsAppAdmin = User.IsAdminOfApp(task.AppId);
            bool userOnTaskIsAppUser = Db.GetAll<User>().Any(u => u.Id == task.ExecuteAs && u.Roles.Any(r => r.Role.AppId == task.AppId));
            bool taskAndFlowFromSameApp = Db.Get<FlowDefinition>(task.FlowId).AppId == task.AppId;
            return userIsAppAdmin && userOnTaskIsAppUser && taskAndFlowFromSameApp;
        }
    }
}