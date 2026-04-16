using cCoder.AppSecurity;
using cCoder.AppSecurity.Exposures.HostedServices;
using cCoder.DocumentManagement;
using cCoder.Mail;
using cCoder.Mail.Exposures.HostedServices;
using cCoder.Scheduling;
using cCoder.Scheduling.Exposures.HostedServices;
using cCoder.Security.Api;
using cCoder.Security.Data.EF.MSSQL;
using cCoder.Security.Services;
using cCoder.Workflow;
using cCoder.Workflow.Exposures.HostedServices;
using cCoder.Core.Cors;

namespace cCoder.Core;

public partial class CoreBuilderOptions
{
    private void ConfigureSecurityAndDataServices()
    {
        EnsureConfiguration();

        string coreConnection = configuration.GetConnectionString("Core");

        services.AddSecurityServices((securityServices, securityConfig) =>
        {
            securityConfig.AddMSSQLModelProvider(
                securityServices,
                configuration.GetConnectionString("SSO"));

            securityConfig.UseAESHMMACPasswordEncryption(
                securityServices,
                configuration.GetSection("settings")["DecryptionKey"]);
        });

        cCoder.Data.IServiceCollectionExtensions.AddCoreData(services, coreConnection);
        services.AddSingleton<ICoreAllowedOriginStore, CoreAllowedOriginStore>();
    }

    private void ConfigureHostedDomainServices()
    {
        services.AddAppSecurity();
        services.AddHostedService<AnalysePlatformUsageHostedService>();
        services.AddHostedService<TokenCleanerHostedService>();
        services.AddDocumentManagement();
        services.AddMail();
        services.AddHostedService<MailSenderHostedService>();
        services.AddScheduling();
        services.AddHostedService<TaskRunnerHostedService>();
        services.AddWorkflow();
        services.AddHostedService<WorkflowInstanceManagementHostedService>();
    }

    private void EnsureConfiguration()
    {
        if (configuration is null)
        {
            throw new InvalidOperationException(
                "You must provide IConfiguration before applying the default core hosted-services baseline.");
        }
    }
}
