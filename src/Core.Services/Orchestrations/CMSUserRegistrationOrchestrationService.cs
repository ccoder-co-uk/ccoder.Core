using Core.Objects;
using Core.Objects.Dtos;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Security;
using Core.Services.Orchestrations.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Core.Services.Orchestrations
{
    public class CMSUserRegistrationOrchestrationService : ICMSUserRegistrationOrchestrationService
    {
        private readonly IAppService appService;
        private readonly ICoreService<User> coreUserService;
        private readonly IUserRoleService userRoleService;
        private readonly IQueuedEmailService queuedEmailService;
        private readonly Config config;

        public CMSUserRegistrationOrchestrationService(
            IAppService appService,
            ICoreService<User> coreUserService,
            IUserRoleService userRoleService,
            IQueuedEmailService queuedEmailService,
            Config config)
        {
            this.appService = appService;
            this.coreUserService = coreUserService;
            this.userRoleService = userRoleService;
            this.queuedEmailService = queuedEmailService;
            this.config = config;
        }

        public async ValueTask<User> RegisterUserAsync(User user, int appId, string confirmationToken)
        {
            var app = appService.GetAll(false)
                .IgnoreQueryFilters()
                .Include(a => a.Roles)
                    .ThenInclude(r => r.Users)
                .Include(a => a.Cultures)
                .Include(a => a.MailServers)
                .Include(a => a.Templates)
                .Include(a => a.Resources)
                .AsSplitQuery()
                .FirstOrDefault(a => a.Id == appId);

            var usersRole = app.Roles
                .FirstOrDefault(r => r.Name == "Users");

            var addedUser = await coreUserService.AddAsync(user);

            bool userIsNotAlreadyInUsersRole = !usersRole.Users
                .Select(ur => ur.UserId)
                .Contains(user.Id);

            if (usersRole != null && userIsNotAlreadyInUsersRole)
                await userRoleService.SaveAsync(new UserRole 
                { 
                    RoleId = usersRole.Id, 
                    UserId = user.Id 
                });

            await SendConfirmRegistrationEmail(confirmationToken, app, user);
            return addedUser;
        }

        async ValueTask SendConfirmRegistrationEmail(string confirmationToken, App app, User user)
        {
            var template = app.Templates
                .FirstOrDefault(t => t.Name == "ConfirmRegistration");

            if (template == null || !app.MailServers.Any())
                return;

            var mailServer = app.MailServers
                .FirstOrDefault(s => s.Name == "Default")
                    ??
                app.MailServers.FirstOrDefault();

            var renderModel = new
            {
                Token = confirmationToken,
                EncodedToken = HttpUtility.UrlEncode(confirmationToken),
                CoreUser = user
            };

            var renderParams = new TemplateRenderParams(app, user, user.DefaultCultureId);

            var confirmRegistrationEmail = template
                .BuildEmailTo(
                    user.Email, 
                    app.Name + ": Confirm Registration", 
                    renderParams, renderModel, 
                    mailServer, 
                    config);

            confirmRegistrationEmail.SentByUserId = user.Id;

            await queuedEmailService.AddAsync(
                confirmRegistrationEmail, 
                checkPrivs: false);
        }
    }
}