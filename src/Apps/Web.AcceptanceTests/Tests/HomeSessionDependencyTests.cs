// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Web.Dependencies;
using Xunit;

namespace Web.AcceptanceTests.Tests;

public sealed partial class HomeSessionDependencyTests
{
    [Fact]
    public void ShouldNotWriteUnchangedSessionValue()
    {
        // Given
        TrackingSession session = new();
        session.SetString(key: "culture", value: "en-GB");
        session.ResetWriteCount();

        DefaultHttpContext context = new();

        context.Features.Set<ISessionFeature>(
            instance: new SessionFeature { Session = session });

        HomeSessionDependency dependency = new(
            httpContextAccessor: new HttpContextAccessor
            {
                HttpContext = context
            });

        // When
        dependency.SetSessionValue(
            context: context,
            key: "Culture",
            value: "en-GB");

        // Then
        session.WriteCount
            .Should()
            .Be(expected: 0);
    }

    [Fact]
    public void ShouldWriteChangedSessionValue()
    {
        // Given
        TrackingSession session = new();
        session.SetString(key: "theme", value: "Default");
        session.ResetWriteCount();

        DefaultHttpContext context = new();

        context.Features.Set<ISessionFeature>(
            instance: new SessionFeature { Session = session });

        HomeSessionDependency dependency = new(
            httpContextAccessor: new HttpContextAccessor
            {
                HttpContext = context
            });

        // When
        dependency.SetSessionValue(
            context: context,
            key: "Theme",
            value: "CorporateLinX");

        // Then
        session.WriteCount
            .Should()
            .Be(expected: 1);

        session.GetString(key: "theme")
            .Should()
            .Be(expected: "CorporateLinX");
    }

    private sealed class SessionFeature : ISessionFeature
    {
        public ISession Session { get; set; }
    }

    private sealed class TrackingSession : ISession
    {
        private readonly Dictionary<string, byte[]> values =
            new(StringComparer.Ordinal);

        public int WriteCount { get; private set; }

        public bool IsAvailable => true;

        public string Id => "tracking-session";

        public IEnumerable<string> Keys => values.Keys;

        public void Clear() =>
            values.Clear();

        public Task CommitAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Remove(string key) =>
            values.Remove(key: key);

        public void Set(string key, byte[] value)
        {
            values[key] = value;
            WriteCount++;
        }

        public bool TryGetValue(string key, out byte[] value) =>
            values.TryGetValue(key: key, value: out value);

        public void ResetWriteCount() =>
            WriteCount = 0;
    }
}