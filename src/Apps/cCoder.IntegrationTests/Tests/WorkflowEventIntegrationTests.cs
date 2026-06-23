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
        FlowDefinition flow = await PostAsJsonAsync<FlowDefinition>("/Api/Core/FlowDefinition", new
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
        });

        return flow.Id;
    }

    private async Task<int> CreateScheduledTaskAsync(Guid flowId, string name)
    {
        ScheduledTask task = await PostAsJsonAsync<ScheduledTask>("/Api/Core/ScheduledTask", new
        {
            appId = BaselineAppId,
            flowId,
            name,
            description = "Integration scheduled task",
            executionArgs = "{}",
            scheduleInTicks = TimeSpan.FromMinutes(5).Ticks,
            executeAs = AdminUserId,
            createdBy = "Guest",
            updatedBy = "Guest",
            created = DateTimeOffset.UtcNow,
            lastUpdated = DateTimeOffset.UtcNow,
            nextExecution = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        return task.Id;
    }

    private async Task DeleteFlowArtifactsAsync(Guid flowId, int taskId = 0)
    {
        if (flowId == Guid.Empty && taskId == 0)
            return;

        await using CoreDataContext core = CreateCoreContext();

        if (taskId != 0)
        {
            ScheduledTask task = await core.Set<ScheduledTask>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(found => found.Id == taskId);

            if (task is not null)
                await core.DeleteAllAsync([task]);
        }

        if (flowId != Guid.Empty)
        {
            await core.DeleteAllAsync(
                core.Set<FlowInstanceData>().IgnoreQueryFilters()
                    .Where(instance => instance.FlowDefinitionId == flowId)
                    .ToArray());

            await core.DeleteAllAsync(
                core.Set<WorkflowEvent>().IgnoreQueryFilters()
                    .Where(workflowEvent => workflowEvent.FlowId == flowId)
                    .ToArray());

            FlowDefinition flow = await core.Set<FlowDefinition>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(found => found.Id == flowId);

            if (flow is not null)
                await core.DeleteAsync(flow);
        }
    }

    private async Task<bool> HasFlowInstanceStateAsync(Guid flowId, string state)
    {
        await using CoreDataContext core = CreateCoreContext();
        return await core.Set<FlowInstanceData>().IgnoreQueryFilters()
            .AnyAsync(instance => instance.FlowDefinitionId == flowId && instance.State == state);
    }

    private async Task<bool> HasAnyFlowInstanceAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();
        return await core.Set<FlowInstanceData>().IgnoreQueryFilters()
            .AnyAsync(instance => instance.FlowDefinitionId == flowId);
    }

    private async Task<FlowInstanceData> GetLatestInstanceAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();
        return await core.Set<FlowInstanceData>().IgnoreQueryFilters()
            .Where(instance => instance.FlowDefinitionId == flowId)
            .OrderByDescending(instance => instance.Start)
            .FirstAsync();
    }

    private async Task PostAsync(string relativeUrl) =>
        await SendWithOptionalHostAsync(HttpMethod.Post, relativeUrl);

    private async Task PostRawAsync(string relativeUrl, string body, string host = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = new StringContent(body ?? string.Empty, System.Text.Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(host))
            request.Headers.Host = host;

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    private async Task<T> PostAsJsonAsync<T>(
        string relativeUrl,
        object payload,
        string authToken = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, relativeUrl)
        {
            Content = JsonContent.Create(payload, options: RequestJsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", authToken);
        }

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException($"Expected payload for {relativeUrl}.");
    }

    private async Task SendWithOptionalHostAsync(HttpMethod method, string relativeUrl, string host = null)
    {
        using HttpRequestMessage request = new(method, relativeUrl);

        if (!string.IsNullOrWhiteSpace(host))
            request.Headers.Host = host;

        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
    }

    private async Task<string> BuildFlowDiagnosticsAsync(Guid flowId)
    {
        await using CoreDataContext core = CreateCoreContext();

        FlowInstanceData[] instances = await core.Set<FlowInstanceData>().IgnoreQueryFilters()
            .Where(instance => instance.FlowDefinitionId == flowId)
            .OrderByDescending(instance => instance.Start)
            .ToArrayAsync();

        string instanceSummary = instances.Length == 0
            ? "No flow instances were found."
            : string.Join(
                Environment.NewLine,
                instances.Select(instance =>
                    $"Instance {instance.Id} | State={instance.State} | Start={instance.Start:u} | End={(instance.End.HasValue ? instance.End.Value.ToString("u") : "<null>")} | Context={instance.ContextString ?? "<null>"}"));

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            [
                "Flow instances:",
                instanceSummary,
                "HostedServices output:",
                TakeLastLines(fixture.HostedServicesOutput, 200),
                "Workflow output:",
                TakeLastLines(fixture.WorkflowOutput, 200),
                "Web output:",
                TakeLastLines(fixture.WebOutput, 200)
            ]);
    }

    private static string TakeLastLines(string content, int maxLines)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "<no output>";

        string[] lines = content
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(Environment.NewLine, lines.TakeLast(maxLines));
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
                return;

            await Task.Delay(delayMilliseconds);
        }

        string diagnostics = diagnosticsFactory is null
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}{await diagnosticsFactory()}";

        throw new TimeoutException($"Timed out waiting for the expected condition.{diagnostics}");
    }

    private CoreDataContext CreateCoreContext() =>
        fixture.DatabaseServices.GetRequiredService<ICoreContextFactory>().CreateCoreContext();

    private async Task<string> CreateAuthTokenAsync(string userId)
    {
        await using DbContext sso = fixture.DatabaseServices
            .GetRequiredService<ISecurityDbContextFactory>()
            .CreateDbContext(true);

        string tokenId = Guid.NewGuid().ToString("N");

        sso.Add(new SsoToken
        {
            Id = tokenId,
            Reason = (int)TokenUse.Auth,
            Expires = DateTimeOffset.UtcNow.AddHours(1),
            UserName = userId
        });

        await sso.SaveChangesAsync();
        return tokenId;
    }

    private static string Unique(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";
}
