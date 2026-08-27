// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Data.Models;
using cCoder.Core.Models;
using cCoder.Eventing.Models;
using cCoder.Security.Models;
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

    public static CoreConfiguration Create(IConfiguration configuration) =>
        Create<CoreConfiguration>(configuration: configuration);

    public static TConfiguration Create<TConfiguration>(
        IConfiguration configuration)
        where TConfiguration : CoreConfiguration, new()
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);

        TConfiguration result = configuration.Get<TConfiguration>() ?? new();
        result.Eventing ??= new EventingConfiguration();
        result.Api ??= new ApiConfiguration();

        ApplyDataConnections(configuration: result);

        result.ApplicationConfiguration = configuration;

        return result;
    }

    private static void ApplyDataConnections(CoreConfiguration configuration)
    {
        string coreConnectionString = FirstConfigured(
            configuration.CoreData?.ConnectionString,
            configuration.AppSecurity?.ConnectionString,
            configuration.ContentManagement?.ConnectionString,
            configuration.DocumentManagement?.ConnectionString,
            configuration.Logging?.ConnectionString,
            configuration.Mail?.ConnectionString,
            configuration.Packaging?.ConnectionString,
            configuration.Workflow?.ConnectionString);

        if (!string.IsNullOrWhiteSpace(value: coreConnectionString))
        {
            configuration.CoreData ??= new CoreDataConfiguration();
            configuration.CoreData.ConnectionString = coreConnectionString;

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

        string securityConnectionString = FirstConfigured(
            configuration.SecurityData?.ConnectionString,
            configuration.Security?.ConnectionString);

        if (!string.IsNullOrWhiteSpace(value: securityConnectionString))
        {
            configuration.SecurityData ??= new SecurityDataConfiguration();
            configuration.SecurityData.ConnectionString =
                securityConnectionString;

            if (configuration.Security is not null)
            {
                configuration.Security.ConnectionString =
                    securityConnectionString;
            }
        }
    }

    private static string FirstConfigured(params string[] values) =>
        values.FirstOrDefault(
            predicate: value => !string.IsNullOrWhiteSpace(value));

}