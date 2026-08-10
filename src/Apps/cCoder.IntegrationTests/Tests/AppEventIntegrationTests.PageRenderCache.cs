// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using cCoder.Data;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.Packaging;
using cCoder.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace cCoder.IntegrationTests.Tests;

public sealed partial class AppEventIntegrationTests
{
    [Fact]
    public async Task PageRenderCacheCrud_ShouldExposeHitMissAndLazyRebuildWithinPerformanceGate()
    {
        // Given
        int appId = 0;
        string appDomain = $"{Unique(prefix: "cache-crud")}.local";

        try
        {
            appId = await CreateStandaloneAppAsync(domain: appDomain);
            await GrantGuestAdminAsync(appId: appId);

            Page[] pages = await SeedMinimalRenderableAppAsync(
                appId: appId,
                originalRenderedOn: DateTimeOffset.UtcNow.AddDays(days: -1));

            Page page = pages[0];

            string cacheId = $"{appId}_{page.Id}__default";

            (HttpResponseMessage warmupResponse, _) =
                await SendCacheRequestAsync(
                    appDomain: appDomain,
                    method: HttpMethod.Get,
                    requestUri: $"/Api/ContentManagement/PageRenderCache('{cacheId}')?$select=Id");

            using (warmupResponse)
            {
                warmupResponse.StatusCode
                    .Should()
                    .Be(expected: HttpStatusCode.OK);
            }

            _ = await RequestPageContentAsync(
                appDomain: appDomain,
                path: pages[1].Path,
                expectedContent: "Old cache");

            // When - existing cache row and cache hit
            (HttpResponseMessage queryResponse, TimeSpan queryDuration) =
                await SendCacheRequestAsync(
                    appDomain: appDomain,
                    method: HttpMethod.Get,
                    requestUri: $"/Api/ContentManagement/PageRenderCache('{cacheId}')?$select=Id");

            using (queryResponse)
            {
                queryResponse.StatusCode
                    .Should()
                    .Be(expected: HttpStatusCode.OK);
            }

            queryDuration.Should()
                .BeLessThan(expected: TimeSpan.FromMilliseconds(milliseconds: 500));

            TimeSpan initialHitDuration = await RequestPageContentAsync(
                appDomain: appDomain,
                path: page.Path,
                expectedContent: "Old cache");

            initialHitDuration.Should()
                .BeLessThan(expected: TimeSpan.FromMilliseconds(milliseconds: 500));

            // When - delete the cache row through its CRUD exposure
            (HttpResponseMessage deleteResponse, TimeSpan deleteDuration) =
                await SendCacheRequestAsync(
                    appDomain: appDomain,
                    method: HttpMethod.Delete,
                    requestUri: $"/Api/ContentManagement/PageRenderCache('{cacheId}')");

            using (deleteResponse)
            {
                deleteResponse.StatusCode
                    .Should()
                    .Be(expected: HttpStatusCode.NoContent);
            }

            deleteDuration.Should()
                .BeLessThan(expected: TimeSpan.FromMilliseconds(milliseconds: 500));

            // Then - one uncached render is returned and the external worker rebuilds the row
            TimeSpan missDuration = await RequestPageContentAsync(
                appDomain: appDomain,
                path: page.Path,
                expectedContent: "Initial content 1",
                unexpectedContent: "Old cache");

            missDuration.Should()
                .BeLessThan(expected: TimeSpan.FromSeconds(seconds: 1));

            await WaitForPageCacheAsync(
                appId: appId,
                pageId: page.Id,
                expectedAppCacheCount: 3);

            (HttpResponseMessage rebuiltQueryResponse, TimeSpan rebuiltQueryDuration) =
                await SendCacheRequestAsync(
                    appDomain: appDomain,
                    method: HttpMethod.Get,
                    requestUri: $"/Api/ContentManagement/PageRenderCache('{cacheId}')?$select=Id");

            using (rebuiltQueryResponse)
            {
                rebuiltQueryResponse.StatusCode
                    .Should()
                    .Be(expected: HttpStatusCode.OK);
            }

            rebuiltQueryDuration.Should()
                .BeLessThan(expected: TimeSpan.FromMilliseconds(milliseconds: 500));

            TimeSpan rebuiltHitDuration = await RequestPageContentAsync(
                appDomain: appDomain,
                path: page.Path,
                expectedContent: "Initial content 1",
                unexpectedContent: "Old cache");

            rebuiltHitDuration.Should()
                .BeLessThan(expected: TimeSpan.FromMilliseconds(milliseconds: 500));
        }
        finally
        {
            if (appId != 0)
            {
                await DeleteAppGraphAsync(appId: appId);
            }
        }
    }

    [Fact]
    public async Task PackageImportComplete_InvalidatesThenLazilyCachesRequestedPages()
    {
        // Given
        int appId = 0;
        string appDomain = $"{Unique(prefix: "cache")}.local";
        DateTimeOffset originalRenderedOn = DateTimeOffset.UtcNow.AddDays(days: -1);

        try
        {
            appId = await CreateStandaloneAppAsync(domain: appDomain);
            await GrantGuestAdminAsync(appId: appId);

            Page[] pages = await SeedMinimalRenderableAppAsync(
                appId: appId,
                originalRenderedOn: originalRenderedOn);

            // When
            Stopwatch importTimer = Stopwatch.StartNew();

            using HttpResponseMessage importResponse = await fixture.WebClient.PostAsJsonAsync(
                requestUri: $"/Api/Core/Package/Import?appId={appId}",
                value: new Package
                {
                    Name = "Minimal cache invalidation",
                    Items =
                    [
                        new PackageItem
                        {
                            Type = "ContentManagement/Resource",
                            Data = JsonSerializer.Serialize(value: new[]
                            {
                                new Resource
                                {
                                    Name = Unique(prefix: "cache-resource"),
                                    Key = "Default",
                                    Culture = string.Empty,
                                    DisplayName = "Cache lifecycle resource",
                                    ShortDisplayName = "Cache lifecycle"
                                }
                            })
                        }
                    ]
                });

            importTimer.Stop();
            string importContent = await importResponse.Content.ReadAsStringAsync();

            // Then
            importResponse.StatusCode.Should()
                .Be(expected: HttpStatusCode.OK, because: BuildFailureMessage(content: importContent));

            importTimer.Elapsed.Should()
                .BeLessThan(expected: TimeSpan.FromSeconds(seconds: 5));

            await WaitUntilAsync(
                predicate: () => HasCacheCountAsync(appId: appId, expectedCount: 0),
                attempts: 50,
                delayMilliseconds: 100,
                diagnosticsFactory: () => BuildPageRenderCacheDiagnosticsAsync(appId: appId));

            TimeSpan firstDuration = await RequestPageAsync(
                appDomain: appDomain,
                path: pages[0].Path,
                expectedContent: "Initial content 1");

            firstDuration.Should()
                .BeLessThan(expected: TimeSpan.FromSeconds(seconds: 5));

            await WaitForPageCacheAsync(
                appId: appId,
                pageId: pages[0].Id,
                expectedAppCacheCount: 1);

            Task<TimeSpan>[] concurrentRequests =
            [
                RequestPageAsync(appDomain: appDomain, path: pages[1].Path, expectedContent: "Initial content 2"),
                RequestPageAsync(appDomain: appDomain, path: pages[1].Path, expectedContent: "Initial content 2"),
                RequestPageAsync(appDomain: appDomain, path: pages[1].Path, expectedContent: "Initial content 2")
            ];

            TimeSpan[] concurrentDurations = await Task.WhenAll(tasks: concurrentRequests);

            concurrentDurations.Should()
                .OnlyContain(predicate: duration => duration < TimeSpan.FromSeconds(seconds: 5));

            await WaitForPageCacheAsync(
                appId: appId,
                pageId: pages[1].Id,
                expectedAppCacheCount: 2);

            (await HasPageCacheAsync(appId: appId, pageId: pages[2].Id)).Should()
                .BeFalse();

            _ = await RequestPageAsync(
                appDomain: appDomain,
                path: pages[2].Path,
                expectedContent: "Initial content 3");

            await WaitForPageCacheAsync(
                appId: appId,
                pageId: pages[2].Id,
                expectedAppCacheCount: 3);

            DateTimeOffset[] settledRenderedOn = await GetRenderedOnValuesAsync(appId: appId);
            await Task.Delay(millisecondsDelay: 750);

            (await GetRenderedOnValuesAsync(appId: appId)).Should()
                .Equal(expected: settledRenderedOn);

            fixture.HostedServicesOutput.Should()
                .NotContain(unexpected: "Stack overflow", because: "cache processing must terminate");
        }
        finally
        {
            if (appId != 0)
            {
                await DeleteAppGraphAsync(appId: appId);
            }
        }
    }

    private async Task<Page[]> SeedMinimalRenderableAppAsync(
        int appId,
        DateTimeOffset originalRenderedOn)
    {
        await using CoreDataContext core = CreateCoreContext();

        await core.AddLayoutAsync(layout: new Layout
        {
            AppId = appId,
            Name = "Default",
            HeaderHtml = "<title>[page[title]]</title>",
            Html = "<main>[content[body]]</main>",
            Script = string.Empty,
            CreatedOn = DateTimeOffset.UtcNow,
            CreatedBy = "Guest",
            LastUpdated = DateTimeOffset.UtcNow,
            LastUpdatedBy = "Guest"
        });

        List<Page> pages = [];

        for (int index = 0; index < 3; index++)
        {
            Page page = await core.AddPageAsync(page: new Page
            {
                AppId = appId,
                Order = index,
                ShowOnMenus = true,
                Name = $"Cache Page {index + 1}",
                Path = index == 0 ? string.Empty : $"cache-{index + 1}",
                ResourceKey = "Default",
                Layout = "Default",
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedBy = "Guest",
                LastUpdated = DateTimeOffset.UtcNow,
                LastUpdatedBy = "Guest"
            });

            pages.Add(item: page);

            await core.AddPageInfoAsync(pageInfo: new PageInfo
            {
                PageId = page.Id,
                CultureId = string.Empty,
                Title = page.Name,
                Description = page.Name,
                Keywords = "acceptance"
            });

            await core.AddContentAsync(content: new Content
            {
                PageId = page.Id,
                CultureId = string.Empty,
                Name = "body",
                Html = $"<p>Initial content {index + 1}</p>"
            });

            core.Set<PageRenderCache>()
                .Add(entity: new PageRenderCache
                {
                    Id = $"{appId}_{page.Id}__default",
                    AppId = appId,
                    PageId = page.Id,
                    Culture = string.Empty,
                    Theme = "default",
                    Path = page.Path,
                    Title = page.Name,
                    Description = page.Name,
                    Keywords = "acceptance",
                    ShowOnMenus = true,
                    Header = "<title>Old cache</title>",
                    Body = "<main>Old cache</main>",
                    SourceFingerprint = "old",
                    RenderedOn = originalRenderedOn
                });

            await core.SaveChangesAsync();
        }

        return [.. pages];
    }

    private Task<TimeSpan> RequestPageAsync(
        string appDomain,
        string path,
        string expectedContent)
        =>
        RequestPageContentAsync(
            appDomain: appDomain,
            path: path,
            expectedContent: expectedContent,
            unexpectedContent: "Old cache");

    private async Task<TimeSpan> RequestPageContentAsync(
        string appDomain,
        string path,
        string expectedContent,
        string unexpectedContent = null)
    {
        using HttpRequestMessage request = new(
            method: HttpMethod.Get,
            requestUri: string.IsNullOrWhiteSpace(value: path) ? "/" : $"/{path}");

        request.Headers.Host = appDomain;
        Stopwatch timer = Stopwatch.StartNew();
        using HttpResponseMessage response = await fixture.WebClient.SendAsync(request: request);
        timer.Stop();
        string content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should()
            .Be(expected: HttpStatusCode.OK, because: BuildFailureMessage(content: content));

        content.Should()
            .Contain(expected: expectedContent);

        if (!string.IsNullOrWhiteSpace(value: unexpectedContent))
        {
            content.Should()
                .NotContain(unexpected: unexpectedContent);
        }

        return timer.Elapsed;
    }

    private async Task<(HttpResponseMessage Response, TimeSpan Duration)>
        SendCacheRequestAsync(
            string appDomain,
            HttpMethod method,
            string requestUri)
    {
        using HttpRequestMessage request = new(
            method: method,
            requestUri: requestUri);

        request.Headers.Host = appDomain;
        Stopwatch timer = Stopwatch.StartNew();

        HttpResponseMessage response = await fixture.WebClient.SendAsync(
            request: request);

        timer.Stop();

        return (response, timer.Elapsed);
    }

    private Task WaitForPageCacheAsync(
        int appId,
        int pageId,
        int expectedAppCacheCount) =>
        WaitUntilAsync(
            predicate: async () =>
                await HasPageCacheAsync(appId: appId, pageId: pageId)
                && await HasCacheCountAsync(appId: appId, expectedCount: expectedAppCacheCount),
            attempts: 50,
            delayMilliseconds: 100,
            diagnosticsFactory: () => BuildPageRenderCacheDiagnosticsAsync(appId: appId));

    private async Task<bool> HasPageCacheAsync(int appId, int pageId)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<PageRenderCache>()
            .AsNoTracking()
            .AnyAsync(predicate: cache => cache.AppId == appId && cache.PageId == pageId);
    }

    private async Task<bool> HasCacheCountAsync(int appId, int expectedCount)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<PageRenderCache>()
            .AsNoTracking()
            .CountAsync(predicate: cache => cache.AppId == appId) == expectedCount;
    }

    private async Task<DateTimeOffset[]> GetRenderedOnValuesAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        return await core.Set<PageRenderCache>()
            .AsNoTracking()
            .Where(predicate: cache => cache.AppId == appId)
            .OrderBy(keySelector: cache => cache.PageId)
            .Select(selector: cache => cache.RenderedOn)
            .ToArrayAsync();
    }

    private async Task<string> BuildPageRenderCacheDiagnosticsAsync(int appId)
    {
        await using CoreDataContext core = CreateCoreContext();

        string[] caches = await core.Set<PageRenderCache>()
            .AsNoTracking()
            .Where(predicate: cache => cache.AppId == appId)
            .OrderBy(keySelector: cache => cache.PageId)
            .Select(selector: cache =>
                $"{cache.Id}: Page={cache.PageId}, RenderedOn={cache.RenderedOn:O}")
            .ToArrayAsync();

        return $"""
            Page render caches:
            {string.Join(separator: Environment.NewLine, values: caches)}

            Web output:
            {Tail(value: fixture.WebOutput)}

            HostedServices output:
            {Tail(value: fixture.HostedServicesOutput)}
            """;
    }
}