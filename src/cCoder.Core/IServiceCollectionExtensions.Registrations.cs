// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models;
using cCoder.AI;
using cCoder.AppSecurity;
using cCoder.ContentManagement;
using cCoder.DocumentManagement;
using cCoder.Logging;
using cCoder.Mail;
using cCoder.Workflow;
using cCoder.Core.Brokers.AppSecurity;
using cCoder.Core.Brokers.ContentManagement;
using cCoder.Core.Brokers.DocumentManagement;
using cCoder.Core.Brokers.Eventing;
using cCoder.Core.Brokers.Http;
using cCoder.Core.Brokers.Mail;
using cCoder.Core.Brokers.Planning;
using cCoder.Core.Brokers.Packaging;
using cCoder.Core.Brokers.Workflow;
using cCoder.Core.Exposures.Managers;
using cCoder.Core.Exposures.PackageManagers;
using cCoder.Core.Exposures.Controllers;
using cCoder.Core.Exposures.Cors;
using cCoder.Core.Exposures.Setup;
using cCoder.Core.Dependencies.Formatters;
using cCoder.Core.Dependencies.Middleware;
using cCoder.Core.Dependencies.OData;
using cCoder.Core.Dependencies.Sessions;
using cCoder.Core.Exposures;
using cCoder.Core.Services.Aggregations;
using cCoder.Core.Services.Aggregations.Packages;
using cCoder.Core.Services.Foundations.AllowedOrigins;
using cCoder.Core.Services.Foundations.AppSecurity;
using cCoder.Core.Services.Foundations.ContentManagement;
using cCoder.Core.Services.Foundations.DocumentManagement;
using cCoder.Core.Services.Foundations.Eventing;
using cCoder.Core.Services.Foundations.Mail;
using cCoder.Core.Services.Foundations.Planning;
using cCoder.Core.Services.Foundations.Workflow;
using cCoder.Core.Services.Orchestrations;
using cCoder.Core.Services.Processings.AllowedOrigins;
using cCoder.Core.Services.Processings.Packages;
using cCoder.Core.Services.Processings.Setup;
using cCoder.Core.Services.Foundations.Setup;
using cCoder.Core.Services.Foundations.TemplatedEmails;
using cCoder.Core.Brokers.Setup;
using cCoder.Core.Services.Setup;
using cCoder.Data;
using cCoder.Data.Models;
using cCoder.Data.Models.Packaging;
using cCoder.Eventing;
using cCoder.Eventing.AzureServiceBus;
using cCoder.Eventing.Http;
using cCoder.Eventing.Models;
using cCoder.Packaging;
using cCoder.Security.Models.Events;
using cCoder.Security;
using cCoder.Security.Data.EF;
using cCoder.Security.Exposures;
using cCoder.Security.Services.Orchestrations.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection AddCoreWeb(
        this IServiceCollection services,
        CoreConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);
        services.AddSingleton(implementationInstance: configuration);

        ODataConventionModelBuilder domainModelBuilder = new();

        services.AddAIWeb(configuration: configuration.AI);
        services.AddSecurityWeb(configuration: configuration.Security);

        services.AddAppSecurityWeb(
            configuration: configuration.AppSecurity,
            builder: domainModelBuilder);

        services.AddDocumentManagementWeb(
            configuration: configuration.DocumentManagement,
            builder: domainModelBuilder);

        services.AddLoggingWeb(
            configuration: configuration.Logging,
            builder: domainModelBuilder);

        services.AddMailWeb(
            configuration: configuration.Mail,
            builder: domainModelBuilder);

        services.AddWorkflowWeb(
            configuration: configuration.Workflow,
            builder: domainModelBuilder);

        services.AddContentManagementWeb(
            configuration: configuration.ContentManagement,
            builder: domainModelBuilder);

        services.AddPackaging(configuration: configuration.Packaging);

        services.AddCoreEventing(
            eventProviders: configuration.Eventing.EventProviders);

        services.AddConfiguredWebEventing(
            configuration: configuration.Eventing);

        services.AddBrokers();
        services.AddFoundations();
        services.AddProcessings();
        services.AddAggregations();
        services.AddOrchestrations();
        services.AddExposures();

        services.AddServiceBusEventForwarding(
            configuration: configuration.Eventing);

        services.AddCoreApiContexts();

        services.AddCoreApiDocumentation(
            apiContexts:
            [
                "Core",
                "AppSecurity",
                "ContentManagement",
                "DocumentManagement",
                "Logging",
                "Mail",
                "Security",
                "Workflow",
            ]);

        services.AddCoreFirstTimeSetup();

        return services;
    }

    private static void AddCoreApiContexts(
        this IServiceCollection services)
    {
        string[] contextNames =
        [
            "Core",
            "AppSecurity",
            "ContentManagement",
            "DocumentManagement",
            "Logging",
            "Mail",
            "Security",
            "Workflow",
        ];

        foreach (string contextName in contextNames)
        {
            services.AddSingleton(
                implementationInstance: new ApiInfo
                {
                    Kind = "Context",
                    Name = contextName,
                    Url = contextName,
                    SwaggerDef =
                        $"/swagger/{contextName}/swagger.json",
                });
        }
    }

    public static IServiceCollection AddCoreHostedServices(
        this IServiceCollection services,
        CoreConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(argument: configuration);
        services.AddSingleton(implementationInstance: configuration);

        services.AddSecurityHostedServices(
            configuration: configuration.Security);

        services.AddAppSecurityHostedServices(
            configuration: configuration.AppSecurity);

        services.AddDocumentManagementHostedServices(
            configuration: configuration.DocumentManagement);

        services.AddLoggingHostedServices(
            configuration: configuration.Logging);

        services.AddMailHostedServices(configuration: configuration.Mail);

        services.AddWorkflowHostedServices(
            configuration: configuration.Workflow);

        services.AddContentManagementHostedServices(
            configuration: configuration.ContentManagement);

        services.AddPackaging(configuration: configuration.Packaging);

        services.AddCoreEventing(
            eventProviders: configuration.Eventing.EventProviders);

        services.AddConfiguredHostedServicesEventing(
            configuration: configuration.Eventing);

        services.AddBrokers();
        services.AddFoundations();
        services.AddProcessings();
        services.AddAggregations();
        services.AddOrchestrations();
        services.AddExposures();

        services.AddServiceBusEventForwarding(
            configuration: configuration.Eventing);

        return services;
    }

    internal static IServiceCollection AddCoreApi(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        services.AddBrokers();
        services.AddFoundations();
        services.AddProcessings();
        services.AddAggregations();
        services.AddOrchestrations();

        return services.AddExposures(
            routeDefinitions: routeDefinitions);
    }

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        params string[] apiContexts)
    {
        CoreApiRouteDefinition[] routes = services.GetRouteDefinitions(apiContexts: apiContexts);
        return services.AddCoreApiDocumentation(routes: routes);
    }

    internal static IServiceCollection AddCoreApiDocumentation(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition[] definitions = services.GetRouteDefinitions(routes: routes);

        services.AddSwaggerGen(setupAction: c =>
        {
            c.ResolveConflictingActions(resolver: apiDescriptions => apiDescriptions.First());
            c.CustomSchemaIds(schemaIdSelector: type => type.FullName?.Replace(oldChar: '+', newChar: '.') ?? type.Name);
            services.AddSwaggerDocuments(options: c, routes: definitions);

            c.DocInclusionPredicate(
predicate: (documentName, apiDescription) =>
                    services.ShouldIncludeInDocument(documentName: documentName, relativePath: apiDescription.RelativePath, routes: definitions));

            c.AddSecurityDefinition(name: "bearer", securityScheme: new OpenApiSecurityScheme
            {
                Description = @"Authorization header using the Bearer scheme. \r\n\r\n 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        \r\n\r\nExample: 'bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
            });
        });

        return services;
    }

    internal static void AddCoreEventing(
        this IServiceCollection services,
        IEnumerable<EventProvider> eventProviders)
    {
        services.AddEventing(configure: configuration =>
        {
            configuration.EventProviders =
                [.. (eventProviders ?? []).Where(predicate: provider => provider is not null)];
        });

        services.AddEventingForType<SecurityAccountEvent>();
    }

    private static IServiceCollection AddConfiguredWebEventing(
        this IServiceCollection services,
        EventingConfiguration configuration)
    {
        if (string.Equals(
                a: configuration.ProviderType,
                b: "Http",
                comparisonType: StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(
                value: configuration.Http.HubUrl))
        {
            services.AddHttpEventingWeb(configure: options =>
            {
                options.HubUrl = configuration.Http.HubUrl;
                options.MaxConcurrency = configuration.Http.MaxConcurrency;
            });
        }
        else if (string.Equals(
                a: configuration.ProviderType,
                b: "ServiceBus",
                comparisonType: StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(
                value: configuration.ServiceBus.ConnectionString))
        {
            services.AddAzureServiceBusEventingWeb(configure: options =>
            {

                options.ConnectionString =
                    configuration.ServiceBus.ConnectionString;

                options.MaxConcurrency =
                    configuration.ServiceBus.MaxConcurrency;
            });
        }

        return services;
    }

    private static IServiceCollection AddConfiguredHostedServicesEventing(
        this IServiceCollection services,
        EventingConfiguration configuration)
    {
        if (string.Equals(
                a: configuration.ProviderType,
                b: "Http",
                comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpEventingHostedServices(configure: options =>
            {
                options.HubUrl = configuration.Http.HubUrl;
                options.MaxConcurrency = configuration.Http.MaxConcurrency;
            });
        }
        else if (string.Equals(
                a: configuration.ProviderType,
                b: "ServiceBus",
                comparisonType: StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(
                value: configuration.ServiceBus.ConnectionString))
        {
            services.AddAzureServiceBusEventingHostedServices(
                configure: options =>
                {

                    options.ConnectionString =
                        configuration.ServiceBus.ConnectionString;

                    options.MaxConcurrency =
                        configuration.ServiceBus.MaxConcurrency;
                });
        }

        return services;
    }

    private static void AddBrokers(
        this IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppBroker, ContentManagementAppBroker>();
        services.AddTransient<IAppGraphEventBroker, AppGraphEventBroker>();
        services.AddTransient<IAuthInfoBroker, AuthInfoBroker>();
        services.AddTransient<IHttpRequestBroker, HttpRequestBroker>();
        services.AddTransient<IAppSecurityAppBroker, AppSecurityAppBroker>();
        services.AddTransient<IPlanningAppBroker, PlanningAppBroker>();
        services.AddTransient<IDocumentManagementAppBroker, DocumentManagementAppBroker>();

        services.AddTransient<IWorkflowAppBroker, WorkflowAppBroker>();
        services.AddTransient<IMailAppBroker, MailAppBroker>();
        services.AddTransient<IMailManagerBroker, MailManagerBroker>();

        services.TryAddTransient<cCoder.Packaging.Exposures.PackageManagers.IAppDomainManager, AppDomainManager>();
        services.TryAddTransient<cCoder.Packaging.Exposures.PackageManagers.IAppSecurityPackageManager, AppSecurityPackageManager>();

        services.TryAddTransient<
            cCoder.Packaging.Exposures.PackageManagers.IContentManagementPackageManager,
            ContentManagementPackageManager>();

        services.TryAddTransient<
            cCoder.Packaging.Exposures.PackageManagers.IDocumentManagementPackageManager,
            DocumentManagementPackageManager>();

        services.TryAddTransient<cCoder.Packaging.Exposures.PackageManagers.ISchedulingPackageManager, SchedulingPackageManager>();
        services.TryAddTransient<cCoder.Packaging.Exposures.PackageManagers.IWorkflowPackageManager, WorkflowPackageManager>();
    }

    private static void AddFoundations(
        this IServiceCollection services)
    {
        services.AddTransient<IContentManagementAppService, ContentManagementAppService>();
        services.AddTransient<IAppGraphEventService, AppGraphEventService>();
        services.AddTransient<IAllowedOriginStoreService, AllowedOriginStoreService>();
        services.AddTransient<IAppSecurityAppService, AppSecurityAppService>();

        services.AddTransient<
            IAppSecurityUserRoleService,
            AppSecurityUserRoleService>();

        services.AddTransient<IPlanningAppService, PlanningAppService>();
        services.AddTransient<IDocumentManagementAppService, DocumentManagementAppService>();
        services.AddTransient<IWorkflowAppService, WorkflowAppService>();
        services.AddTransient<IMailAppService, MailAppService>();
        services.AddTransient<IMailManagerService, MailManagerService>();
        services.AddTransient<IPackageBroker, PackageBroker>();

        services.AddTransient<
            ITemplatedEmailContentService,
            TemplatedEmailContentService>();

        services.AddTransient<
            ITemplatedEmailIdentityService,
            TemplatedEmailIdentityService>();

        services.AddTransient<
            ITemplatedEmailQueueService,
            TemplatedEmailQueueService>();
    }

    private static void AddServiceBusEventForwarding(
        this IServiceCollection services,
        EventingConfiguration configuration)
    {
        if (!string.Equals(
                a: configuration.ProviderType,
                b: "ServiceBus",
                comparisonType: StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(
                value: configuration.ServiceBus.ConnectionString))
        {
            return;
        }

        services.AddTransient<
            IServiceBusAppDeleteForwardingBroker,
            ServiceBusAppDeleteForwardingBroker>();

        services.AddTransient<
            IServiceBusFolderDeleteForwardingBroker,
            ServiceBusFolderDeleteForwardingBroker>();

        services.AddTransient<ServiceBusAppDeleteForwardingService>();
        services.AddTransient<ServiceBusFolderDeleteForwardingService>();
    }

    private static void AddProcessings(
        this IServiceCollection services)
    {
        services.AddTransient<
            IAllowedOriginStoreProcessingService,
            AllowedOriginStoreProcessingService>();

        services.AddTransient<
            IAppSecurityPackageProcessingService,
            AppSecurityPackageProcessingService>();

        services.AddTransient<
            IContentManagementPackageProcessingService,
            ContentManagementPackageProcessingService>();

        services.AddTransient<
            IDocumentManagementPackageProcessingService,
            DocumentManagementPackageProcessingService>();

        services.AddTransient<
            ISchedulingPackageProcessingService,
            SchedulingPackageProcessingService>();

        services.AddTransient<
            IWorkflowPackageProcessingService,
            WorkflowPackageProcessingService>();
    }

    private static void AddAggregations(
        this IServiceCollection services)
    {
        services.AddTransient<IAppAggregationService, AppAggregationService>();
        services.AddTransient<ICoreAppManager, AppAggregationService>();
        services.AddTransient<IUserRegistrationAggregationService, UserRegistrationAggregationService>();

        services.AddTransient<
            IPackageManagerAggregationService,
            PackageManagerAggregationService>();

        services.AddTransient<
            IPackageManager,
            PackageManagerAggregationService>();

        services.AddTransient<
            ISecurityAccountEmailAggregationService,
            SecurityAccountEmailAggregationService>();
    }

    private static void AddOrchestrations(
        this IServiceCollection services)
    {
        services.AddTransient<
            IAppOrchestrationService,
            AppAggregationService>();

        services.AddTransient<
            ITemplatedEmailOperationOrchestrationService,
            TemplatedEmailOperationOrchestrationService>();

        services.AddTransient<
            IHostedServicesAppSecurityAppAddOrchestrationService,
            HostedServicesAppSecurityAppAddOrchestrationService>();
    }

    private static IServiceCollection AddCoreFirstTimeSetup(
        this IServiceCollection services)
    {
        EnsureFirstTimeSetupSecurityManagers(services: services);
        services.AddScoped<ICoreSetupContextBroker, CoreSetupContextBroker>();
        services.AddScoped<ISecuritySetupContextBroker, SecuritySetupContextBroker>();
        services.AddScoped<ICoreSetupStateService, CoreSetupStateService>();
        services.AddScoped<ISecuritySetupStateService, SecuritySetupStateService>();
        services.AddScoped<IAppSecurityAppService, AppSecurityAppService>();
        services.AddScoped<IAppSecurityUserRoleService, AppSecurityUserRoleService>();

        services.AddScoped<
            IFirstTimeSetupStateOrchestrationService,
            FirstTimeSetupStateOrchestrationService>();

        services.AddScoped<IFirstTimeSetupStateService, FirstTimeSetupStateManager>();
        services.AddScoped<IFirstTimeSetupManager, FirstTimeSetupStateManager>();
        services.AddScoped<ISetupRequestHostProcessingService, SetupRequestHostProcessingService>();
        services.AddScoped<ISetupRequestHostManager, SetupRequestHostManager>();
        IMvcBuilder mvcBuilder = services.AddMvc();

        mvcBuilder.AddApplicationPart(
            assembly: typeof(SetupController).Assembly);

        return services;
    }

    private static void EnsureFirstTimeSetupSecurityManagers(
        this IServiceCollection services)
    {
        if (!services.Any(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITokenManager)))
        {
            Type tokenManagerType = Type.GetType(
                typeName:
                    "cCoder.Security.Exposures.TokenManager, cCoder.Security");

            if (tokenManagerType is not null)
            {
                services.AddTransient(
                    serviceType: typeof(ITokenManager),
                    implementationType: tokenManagerType);
            }
        }

        if (!services.Any(predicate: descriptor =>
                descriptor.ServiceType == typeof(ITenantManager)))
        {
            Type tenantManagerType = Type.GetType(
                typeName:
                    "cCoder.Security.Exposures.TenantManager, cCoder.Security");

            if (tenantManagerType is not null)
            {
                services.AddTransient(
                    serviceType: typeof(ITenantManager),
                    implementationType: tenantManagerType);
            }
        }
    }

    private static IServiceCollection AddExposures(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions = null)
    {
        services.AddCoreAspNetExposures();

        services.AddTransient<
            ITemplatedEmailManager,
            TemplatedEmailManager>();

        services.AddTransient<
            ITemplatedEmailOrchestrationService,
            TemplatedEmailManager>();

        services.AddTransient<
            IUserRegistrationOrchestrationService,
            UserRegistrationManager>();

        services.AddScoped<ICoreAllowedOriginStore, CoreAllowedOriginStore>();

        AddCoreODataExposures(
            services: services,
            routeDefinitions: routeDefinitions);

        AddCoreODataRouteMode(services: services);

        return services;
    }

    private static void AddCoreAspNetExposures(
        this IServiceCollection services)
    {
        CoreConfiguration coreConfiguration =
            services.GetRegisteredCoreConfiguration();

        services.AddRouting();
        services.AddResponseCompression();
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddTransient<CoreFormatterMiddleware>();
        services.AddTransient<CoreExceptionMiddleware>();

        services.AddScoped(
            serviceType: typeof(HttpContext),
            implementationFactory: context =>
                services.CreateHttpContext(
                    httpContext: context
                        .GetService<IHttpContextAccessor>()
                        ?.HttpContext));

        services.AddScoped(
            serviceType: typeof(HttpRequest),
            implementationFactory: context =>
                context.GetRequiredService<HttpContext>().Request);

        services.AddScoped(
            serviceType: typeof(ISession),
            implementationFactory: context =>
            {
                HttpContext httpContext =
                    context.GetRequiredService<HttpContext>();

                return httpContext.Features
                    .Get<ISessionFeature>()
                    ?.Session
                    ?? NoOpSession.Instance;
            });

        services.AddSession(configure: options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;
        });

        services.AddHsts(configureOptions: options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromMinutes(minutes: 60);
        });

        services.AddMvc(setupAction: options =>
        {
            options.EnableEndpointRouting = false;
            options.OutputFormatters.Add(item: new XmlFormatter());
            options.OutputFormatters.Add(item: new CsvFormatter());
            options.OutputFormatters.Add(item: new ExcelFormatter());

            if (coreConfiguration?.AppSecurity.AggregateDomains != true)
            {
                options.Conventions.Add(
                    actionModelConvention:
                        new SplitDomainApplicationModelConvention());
            }
        });

        services.AddRazorPages();

        services.Configure<KestrelServerOptions>(
            configureOptions: options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });

        services.AddEndpointsApiExplorer();
        services.AddSignalR();
    }

    private static void AddCoreODataExposures(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routeDefinitions)
    {
        DefaultODataBatchHandler batchHandler = new();
        ODataConventionModelBuilder packagingModelBuilder = new();

        packagingModelBuilder.EntitySet<Package>(name: nameof(Package));
        packagingModelBuilder.EntitySet<PackageItem>(name: nameof(PackageItem));
        packagingModelBuilder.Namespace = string.Empty;

        IEdmModel packagingModel = packagingModelBuilder.GetEdmModel();

        CoreApiRouteDefinition[] definitions = [.. (routeDefinitions ?? [])
            .Where(predicate: route =>
                route is not null
                && (string.Equals(
                        a: route.Name,
                        b: "Core",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.RoutePath,
                        b: "Api/Core",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.Name,
                        b: "Security",
                        comparisonType: StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        a: route.RoutePath,
                        b: "Api/Security",
                        comparisonType: StringComparison.OrdinalIgnoreCase)))];

        IMvcBuilder mvcBuilder = services.AddControllers();

        mvcBuilder.AddOData(setupAction: options =>
        {
            options.RouteOptions.EnableQualifiedOperationCall = false;
            options.EnableAttributeRouting = true;
            options.RouteOptions.EnableKeyAsSegment = false;

            options.Expand()
                .Count()
                .Filter()
                .Select()
                .OrderBy()
                .SetMaxTop(maxTopValue: 1000);

            _ = options.AddRouteComponents(
                routePrefix: "Api/Packaging",
                model: packagingModel,
                batchHandler: batchHandler);

            foreach (CoreApiRouteDefinition routeDefinition in definitions)
            {
                _ = options.AddRouteComponents(
                    routePrefix: routeDefinition.RoutePath,
                    model: routeDefinition.RouteModel,
                    batchHandler: batchHandler);
            }
        });
    }

    private static void AddCoreODataRouteMode(
        this IServiceCollection services)
    {
        CoreConfiguration coreConfiguration =
            services.GetRegisteredCoreConfiguration();

        services.PostConfigure<ODataOptions>(
            configureOptions: options =>
            {
                if (coreConfiguration?.AppSecurity.AggregateDomains != true)
                {
                    return;
                }

                string[] aggregateDomainRoutes =
                [
                    "Api/AppSecurity",
                    "Api/ContentManagement",
                    "Api/DocumentManagement",
                    "Api/Logging",
                    "Api/Mail",
                    "Api/Workflow",
                ];

                foreach (string route in aggregateDomainRoutes)
                {
                    options.RouteComponents.Remove(key: route);
                }
            });
    }

    private static CoreConfiguration GetRegisteredCoreConfiguration(
        this IServiceCollection services) =>
        services
            .Where(predicate: descriptor =>
                descriptor.ServiceType == typeof(CoreConfiguration))
            .Select(selector: descriptor =>
                descriptor.ImplementationInstance)
            .OfType<CoreConfiguration>()
            .LastOrDefault();

    private static HttpContext CreateHttpContext(
        this IServiceCollection services,
        HttpContext httpContext)
    {
        if (httpContext is not null)
        {
            return httpContext;
        }

        DefaultHttpContext fallbackContext = new();

        fallbackContext.Features.Set<ISessionFeature>(
            instance: new NoOpSessionFeature());

        return fallbackContext;
    }

    private static CoreApiRouteDefinition[] GetRouteDefinitions(
        this IServiceCollection services,
        IEnumerable<string> apiContexts) =>
        services.GetRouteDefinitions(routes: (apiContexts ?? [])
            .Where(predicate: context => !string.IsNullOrWhiteSpace(value: context))
            .Select(selector: context => new CoreApiRouteDefinition(
                context,
                $"Api/{context}",
                null)));

    private static CoreApiRouteDefinition[] GetRouteDefinitions(
        this IServiceCollection services,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        CoreApiRouteDefinition coreRoute = new("Core", "Api/Core", null);

        return [coreRoute, .. (routes ?? [])
            .Where(predicate: route => route is not null && !string.IsNullOrWhiteSpace(value: route.Name))
            .GroupBy(keySelector: route => route.Name,comparer: StringComparer.OrdinalIgnoreCase)
            .Select(selector: group => group.First())
            .Where(predicate: route => !string.Equals(a: route.Name,b: "Core",comparisonType: StringComparison.OrdinalIgnoreCase))];
    }

    private static void AddSwaggerDocuments(
        this IServiceCollection services,
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        foreach (CoreApiRouteDefinition route in routes)
        {
            options.SwaggerDoc(name: route.Name, info: new OpenApiInfo
            {
                Title = $"{route.Name} API definition",
                Version = route.Name,
            });
        }

        options.SwaggerDoc(name: "v1", info: new OpenApiInfo
        {
            Title = "Corporate LinX V7 API definition",
            Version = "v1",
        });
    }

    private static bool ShouldIncludeInDocument(
        this IServiceCollection services,
        string documentName,
        string relativePath,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (string.IsNullOrWhiteSpace(value: relativePath))
        {
            return false;
        }

        if (string.Equals(a: documentName, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            documentName = "Core";
        }

        string path = services.NormalizePath(relativePath: relativePath);

        if (string.Equals(a: documentName, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return services.IsCoreRoute(path: path, routes: routes);
        }

        CoreApiRouteDefinition route = routes.FirstOrDefault(predicate: candidate =>
            string.Equals(a: candidate.Name, b: documentName, comparisonType: StringComparison.OrdinalIgnoreCase));

        return route is not null && services.MatchesRoutePath(path: path, routePath: route.RoutePath);
    }

    private static bool IsCoreRoute(
        this IServiceCollection services,
        string path,
        IEnumerable<CoreApiRouteDefinition> routes)
    {
        if (services.MatchesContextRoute(path: path, context: "Core"))
        {
            return true;
        }

        if (!path.Equals(value: "/Api", comparisonType: StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith(value: "/Api/", comparisonType: StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        foreach (CoreApiRouteDefinition route in routes.Where(predicate: route =>
                     !string.Equals(a: route.Name, b: "Core", comparisonType: StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(a: route.Name, b: "v1", comparisonType: StringComparison.OrdinalIgnoreCase)))
        {
            if (services.MatchesRoutePath(path: path, routePath: route.RoutePath))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesRoutePath(
        this IServiceCollection services,
        string path,
        string routePath)
    {
        string prefix = services.NormalizePath(relativePath: routePath);

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesContextRoute(
        this IServiceCollection services,
        string path,
        string context)
    {
        string prefix = $"/Api/{context}";

        return path.Equals(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(value: $"{prefix}/", comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(
        this IServiceCollection services,
        string relativePath) =>
        relativePath.StartsWith(value: '/') ? relativePath : $"/{relativePath}";
}
