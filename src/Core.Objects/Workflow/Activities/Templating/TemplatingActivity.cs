using Core.Objects.Dtos;
using Core.Objects.Entities.CMS;
using Core.Objects.Entities.Mail;
using Core.Objects.Entities.Security;
using Core.Objects.Extensions;
using Core.Objects.Workflow.Activities.Api;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Templating
{
    public abstract class TemplatingActivity<T> : ApiActivity
    {
        public int AppId { get; set; }

        public string Culture { get; set; }

        public string TemplateName { get; set; }

        public T Data { get; set; }

        public string Result { get; set; }

        public User User { get; set; }


        public override Task ExecuteInternal(IWorkflowContext context)
        {
            AppId = (int)context.Variables["AppId"];
            return base.ExecuteInternal(context);
        }

        protected async Task<App> GetApp(HttpClient api)
            => await api.Get<App>($"Core/App({AppId})?$expand=Resources,Templates,MailServers");
        protected async Task<string> Render(HttpClient api)
        {
            try
            {
                App app = await GetApp(api);
                Template template = app.Templates.FirstOrDefault(t => t.Name == TemplateName);
                if (template == null)
                    Log(Dtos.Workflow.WorkflowLogLevel.Error, "Template could be found.");
                else
                    return template.Render(Data, new TemplateRenderParams(app, User, Culture));
            }
            catch (Exception ex)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Error, "Template could not be rendered.\n" + ex.Message);
            }

            return string.Empty;
        }

        protected async Task<QueuedEmail> BuildEmailTo(string emailAddress, string subject, string serverName, HttpClient api)
        {
            try
            {
                App app = await GetApp(api);
                Template template = app.Templates.FirstOrDefault(t => t.Name == TemplateName);
                MailServer serverInfo = app.MailServers.FirstOrDefault(s => s.Name == serverName);

                if (template == null)
                    throw new InvalidOperationException("Template could not be found.");

                if (serverInfo == null)
                    throw new InvalidOperationException("Mail Server configuration could not be found.");

                return template.BuildEmailTo(
                    emailAddress,
                    subject,
                    new TemplateRenderParams(app, User, Culture),
                    Data,
                    serverInfo
                );
            }
            catch (Exception ex)
            {
                Log(Dtos.Workflow.WorkflowLogLevel.Warning, "Template could not be rendered.\n" + ex.Message + "\n - " + ex.StackTrace);
                return null;
            }
        }
    }
}