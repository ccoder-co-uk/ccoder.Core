// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using cCoder.Data;
using cCoder.Data.Models.Planning;
using cCoder.Data.Models.Workflow;
using cCoder.IntegrationTests.Infrastructure;
using cCoder.Security.Data.EF.Interfaces;
using cCoder.Security.Objects.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SsoToken = cCoder.Security.Objects.Entities.Token;

namespace cCoder.IntegrationTests.Tests;

[Collection(IntegrationAcceptanceCollection.Name)]
public sealed partial class WorkflowEventIntegrationTests
{
    private const int BaselineAppId = 1;
    private const string AdminUserId = "admin";
    private const string SimpleFlowDefinitionJson =
        "{\"Name\":\"Acceptance\",\"Activities\":[{\"$type\":\"cCoder.Workflow.Activities.Start, cCoder.Workflow.Activities\",\"Ref\":\"start\"}],\"Links\":[]}";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions RequestJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IntegrationAcceptanceFixture fixture;

    public WorkflowEventIntegrationTests(IntegrationAcceptanceFixture fixture) =>
        this.fixture = fixture;

    private async Task<Guid> CreateFlowDefinitionAsync(int appId, string name)
    {
        FlowDefinition flow = (FlowDefinition)await PostAsJsonAsync(
            relativeUrl: "/Api/Workflow/FlowDefinition",
            payload: new
        {
            appId,
            name,
            description = "Integration flow",
            definitionJson = SimpleFlowDefinitionJson,
            configJson = "{}",
            createdBy = "Guest",
            createdOn = DateTimeOffset.UtcNow,
            lastUpdatedBy = "Guest",
            lastUpdated = DateTimeOffset.UtcNow
            },
            responseType: typeof(FlowDefinition));

        return flow.Id;
    }

    private async Task<int> CreateScheduledTaskAsync(
        Guid flowId,
        string name,
        DateTimeOffset? nextExecution = null,
        string executeAs = null)
    {
        ScheduledTask task = (ScheduledTask)await PostAsJsonAsync(
            relativeUrl: "/Api/Workflow/ScheduledTask",
            payload: new
        {
            appId = BaselineAppId,
            flowId,
            name,
            description = "Integration scheduled task",
            executionArgs = "{}",
            scheduleInTicks = TimeSpan.FromMinutes(minutes: 5).Ticks,
            executeAs = executeAs ?? AdminUserId,
            createdBy = "Guest",
            updatedBy = "Guest",
            created = DateTimeOffset.UtcNow,
            lastUpdated = DateTimeOffset.UtcNow,
            nextExecution = nextExecution ?? DateTimeOffset.UtcNow.AddMinutes(minutes: 5)
            },
            responseType: typeof(ScheduledTask));

        return task.Id;
    }

    private async Task DeleteFlowArtifactsAsync(Guid flowId, int taskId = 0)
    {
        if (flowId == Guid.Empty && taskId == 0)
        {
            return;
        }

        await using CoreDataContext core = CreateCoreContext();

        if (taskId != 0)
        {
            ScheduledTask task = await core.Set<ScheduledTask>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == taskId);

            if (task is not null)
            {
                await core.DeleteAllAsync(scheduledTasks: [task]);
            }
        }

        if (flowId != Guid.Empty)
        {
            await core.DeleteAllAsync(
flowInstances:                 core.Set<FlowInstanceData>()
                .IgnoreQueryFilters()
                    .Where(predicate: instance => instance.FlowDefinitionId == flowId)
                    .ToArray());

            await core.DeleteAllAsync(
workflowEvents:                 core.Set<WorkflowEvent>()
                .IgnoreQueryFilters()
                    .Where(predicate: workflowEvent => workflowEvent.FlowId == flowId)
                    .ToArray());

            FlowDefinition flow = await core.Set<FlowDefinition>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == flowId);

            if (flow is not null)
            {
                await core.DeleteAsync(flowDefinition: flow);
            }
        }
    }

    private async Task<(string userId, Guid roleId)> CreateExecuteOnlyUserAsync(int appId)
    {
        string userId = $"scheduled-{Guid.NewGuid():N}";
        Guid roleId = Guid.NewGuid();

        await using CoreDataContext core = CreateCoreContext();

        await core.AddUserAsync(user: new cCoder.Data.Models.Security.User
        {
            Id = userId,
            DefaultCultureId = "en-GB",
            DisplayName = "Scheduled Execute Only",
            Email = $"{userId}@integration.local",
            IsActive = true
        });

        await core.AddRoleAsync(role: new cCoder.Data.Models.Security.Role
        {
            Id = roleId,
            AppId = appId,
            Name = $"Execute Only {userId}",
            Description = "Integration execute-only role",
            Privs = "flowdefinition_execute"
        });

        await core.AddUserRoleAsync(userRole: new cCoder.Data.Models.Security.UserRole
        {
            RoleId = roleId,
            UserId = userId
        });

        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        sso.Add(entity: new cCoder.Security.Objects.Entities.SSOUser
        {
            Id = userId,
            DisplayName = "Scheduled Execute Only",
            Email = $"{userId}@integration.local",
            EmailConfirmed = true
        });

        await sso.SaveChangesAsync();

        return (userId, roleId);
    }

    private async Task DeleteExecuteOnlyUserAsync(string userId, Guid roleId)
    {
        if (string.IsNullOrWhiteSpace(value: userId) && roleId == Guid.Empty)
        {
            return;
        }

        await using CoreDataContext core = CreateCoreContext();

        if (roleId != Guid.Empty)
        {
            cCoder.Data.Models.Security.UserRole[] userRoles = await core.Set<cCoder.Data.Models.Security.UserRole>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.RoleId == roleId)
                .ToArrayAsync();

            if (userRoles.Length > 0)
            {
                await core.DeleteAllAsync(userRoles: userRoles);
            }

            cCoder.Data.Models.Security.Role role = await core.Set<cCoder.Data.Models.Security.Role>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == roleId);

            if (role is not null)
            {
                await core.DeleteAsync(role: role);
            }
        }

        if (!string.IsNullOrWhiteSpace(value: userId))
        {
            cCoder.Data.Models.Security.User user = await core.Set<cCoder.Data.Models.Security.User>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == userId);

            if (user is not null)
            {
                await core.DeleteAsync(user: user);
            }
        }

        if (!string.IsNullOrWhiteSpace(value: userId))
        {
            await using DbContext sso = fixture.DatabaseServices
                .GetRequiredService<ISecurityDbContextFactory>()
                .CreateDbContext(ignoreAuthInfo: true);

            cCoder.Security.Objects.Entities.Token[] tokens = await sso.Set<cCoder.Security.Objects.Entities.Token>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.UserName == userId)
                .ToArrayAsync();

            if (tokens.Length > 0)
            {
                sso.RemoveRange(entities: tokens);
                await sso.SaveChangesAsync();
            }

            UserEvent[] userEvents = await sso.Set<UserEvent>()
                .IgnoreQueryFilters()
                .Where(predicate: found => found.CreatedBy == userId)
                .ToArrayAsync();

            if (userEvents.Length > 0)
            {
                sso.RemoveRange(entities: userEvents);
                await sso.SaveChangesAsync();
            }

            cCoder.Security.Objects.Entities.SSOUser ssoUser = await sso.Set<cCoder.Security.Objects.Entities.SSOUser>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(predicate: found => found.Id == userId);

            if (ssoUser is not null)
            {
                sso.Remove(entity: ssoUser);
                await sso.SaveChangesAsync();
            }
        }
    }

    private async Task<bool> HasFlowInstanceStateAsync(Guid flowId, string state)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: instance => instance.FlowDefinitionId == flowId && instance.State == state);
    }

    private async Task<bool> HasAnyFlowInstanceAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
            .AnyAsync(predicate: instance => instance.FlowDefinitionId == flowId);
    }

    private async Task<FlowInstanceData> GetLatestInstanceAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
            .Where(predicate: instance => instance.FlowDefinitionId == flowId)
            .OrderByDescending(keySelector: instance => instance.Start)
            .FirstAsync();
    }

    private async Task<FlowInstanceData[]> GetFlowInstancesAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
            .Where(predicate: instance => instance.FlowDefinitionId == flowId)
            .OrderByDescending(keySelector: instance => instance.Start)
            .ToArrayAsync();
    }

    private async Task UpdateScheduledTaskNextExecutionAsync(int taskId, DateTimeOffset nextExecution)
    {
        await using CoreDataContext core = CreateCoreContext();

        ScheduledTask task = await core.Set<ScheduledTask>()
            .IgnoreQueryFilters()
            .FirstAsync(predicate: found => found.Id == taskId);

        task.NextExecution = nextExecution;
        task.LastUpdated = DateTimeOffset.UtcNow;
        task.UpdatedBy = "acceptance";

        _ = await core.SaveChangesAsync();
    }

    private bool HostedServicesOutputContains(string value) =>
        fixture.HostedServicesOutput.Contains(value: value,comparisonType: StringComparison.Ordinal);

    private Task PostAsync(string relativeUrl) =>
        SendWithOptionalHostAsync(method: HttpMethod.Post,relativeUrl: relativeUrl);

    private async Task PostRawAsync(string relativeUrl, string body, string host = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = new StringContent(body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(value: host))
        {
            request.Headers.Host = host;
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);
    }

    private async Task<object> PostAsJsonAsync(
        string relativeUrl,
        object payload,
        Type responseType,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(inputValue: payload,options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(value: authToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authToken);
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);

        return JsonSerializer.Deserialize(json: content, returnType: responseType, options: JsonOptions)
            ?? throw new InvalidOperationException($"Expected payload for {relativeUrl}.");
    }

    private async Task SendWithOptionalHostAsync(HttpMethod method, string relativeUrl, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl);

        if (!string.IsNullOrWhiteSpace(value: host))
        {
            request.Headers.Host = host;
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK,because: content);
    }

    private async Task<string> BuildFlowDiagnosticsAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();

        FlowInstanceData[] instances = await core.Set<FlowInstanceData>()
            .IgnoreQueryFilters()
            .Where(predicate: instance => instance.FlowDefinitionId == flowId)
            .OrderByDescending(keySelector: instance => instance.Start)
            .ToArrayAsync();

        string instanceSummary = instances.Length == 0
            ? "No flow instances were found."
            : string.Join(
separator:                 Environment.NewLine,values:                 instances.Select(selector: instance =>
                    $"Instance {instance.Id} | State={instance.State} | Start={instance.Start:u} | End={(instance.End.HasValue ? instance.End.Value.ToString(format: "u") : "<null>")} | Context={instance.ContextString ?? "<null>"}"));

        return string.Join(
separator:             Environment.NewLine + Environment.NewLine,value:             [
                "Flow instances:",
                instanceSummary,
                "HostedServices output:",
                TakeLastLines(content: fixture.HostedServicesOutput,maxLines: 200),
                "Workflow output:",
                TakeLastLines(content: fixture.WorkflowOutput,maxLines: 200),
                "Web output:",
                TakeLastLines(content: fixture.WebOutput,maxLines: 200)
            ]);
    }

    private static string TakeLastLines(string content, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(value: content))
        {
            return "<no output>";
        }

        string[] lines = content
            .Split(separator: Environment.NewLine,options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(separator: Environment.NewLine,values: lines.TakeLast(count: maxLines));
    }

    private static async Task WaitUntilAsync(
        Func<Task<bool>> predicate,
        int attempts = 60,
        int delayMilliseconds = 500,
        Func<Task<string>> diagnosticsFactory = null)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (await predicate())
            {
                return;
            }

            await Task.Delay(millisecondsDelay: delayMilliseconds);
        }

        string diagnostics = diagnosticsFactory is null
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}{await diagnosticsFactory()}";

        throw new TimeoutException($"Timed out waiting for the expected condition.{diagnostics}");
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>()
            .CreateCoreContext();

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(ignoreAuthInfo: true);

        string tokenId = Guid.NewGuid()
            .ToString(format: "N");

        sso.Add(entity: new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(hours: 1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";
}