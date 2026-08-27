// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.AI.Models.Configurations;
using cCoder.AppSecurity.Models;
using cCoder.ContentManagement.Models;
using cCoder.Data.Models;
using cCoder.Core.Models;
using cCoder.DocumentManagement.Models;
using cCoder.Eventing.Models;
using cCoder.Logging.Models;
using cCoder.Mail.Models;
using cCoder.Packaging.Models;
using cCoder.Security.Models;
using cCoder.Workflow.Models;
using Microsoft.Extensions.Configuration;

namespace cCoder.Core;

public static class CoreConfigurationFactory
{
    public static CoreConfiguration Create() =>
        new()
        {
            Eventing = new EventingConfiguration(),
            Api = new ApiConfiguration(),
        };

    public static CoreConfiguration Create(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);

        CoreConfiguration result = Create();

        result.AI = GetOptional<AIConfiguration>(configuration, nameof(result.AI));
        result.AppSecurity = GetOptional<AppSecurityConfiguration>(configuration, nameof(result.AppSecurity));
        result.ContentManagement = GetOptional<ContentManagementConfiguration>(configuration, nameof(result.ContentManagement));
        result.CoreData = GetOptional<CoreDataConfiguration>(configuration, nameof(result.CoreData));
        result.DocumentManagement = GetOptional<DocumentManagementConfiguration>(configuration, nameof(result.DocumentManagement));
        result.Logging = GetOptional<LoggingConfiguration>(configuration, nameof(result.Logging));
        result.Mail = GetOptional<MailConfiguration>(configuration, nameof(result.Mail));
        result.Packaging = GetOptional<PackagingConfiguration>(configuration, nameof(result.Packaging));
        result.Security = GetOptional<SecurityConfiguration>(configuration, nameof(result.Security));
        result.SecurityData = GetOptional<SecurityDataConfiguration>(configuration, nameof(result.SecurityData));
        result.Workflow = GetOptional<WorkflowConfiguration>(configuration, nameof(result.Workflow));

        configuration.GetSection(key: nameof(result.Eventing))
            .Bind(instance: result.Eventing);

        configuration.GetSection(key: nameof(result.Api))
            .Bind(instance: result.Api);

        ApplyDataConnections(configuration: result);

        result.ApplicationConfiguration = configuration;

        return result;
    }

    private static void ApplyDataConnections(CoreConfiguration configuration)
    {
        string coreConnectionString = configuration.CoreData?.ConnectionString;

        if (!string.IsNullOrWhiteSpace(value: coreConnectionString))
        {
            if (configuration.AppSecurity is not null)
            {
                configuration.AppSecurity.ConnectionString = coreConnectionString;
            }

            if (configuration.ContentManagement is not null)
            {
                configuration.ContentManagement.ConnectionString = coreConnectionString;
            }

            if (configuration.DocumentManagement is not null)
            {
                configuration.DocumentManagement.ConnectionString = coreConnectionString;
            }

            if (configuration.Logging is not null)
            {
                configuration.Logging.ConnectionString = coreConnectionString;
            }

            if (configuration.Mail is not null)
            {
                configuration.Mail.ConnectionString = coreConnectionString;
            }

            if (configuration.Packaging is not null)
            {
                configuration.Packaging.ConnectionString = coreConnectionString;
            }

            if (configuration.Workflow is not null)
            {
                configuration.Workflow.ConnectionString = coreConnectionString;
            }
        }

        if (configuration.Security is not null
            && !string.IsNullOrWhiteSpace(
                value: configuration.SecurityData?.ConnectionString))
        {
            configuration.Security.ConnectionString =
                configuration.SecurityData.ConnectionString;
        }
    }

    private static TConfiguration GetOptional<TConfiguration>(
        IConfiguration configuration,
        string sectionName)
        where TConfiguration : class
    {
        IConfigurationSection section =
            configuration.GetSection(key: sectionName);

        return section.Exists()
            ? section.Get<TConfiguration>()
            : null;
    }
}