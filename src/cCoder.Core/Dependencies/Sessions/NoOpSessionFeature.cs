// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http.Features;

namespace cCoder.Core.Dependencies.Sessions;

public sealed class NoOpSessionFeature : ISessionFeature, ISession
{
    public static NoOpSessionFeature Instance { get; } = new();

    public ISession Session { get; set; }

    public IEnumerable<string> Keys => [];

    public string Id => string.Empty;

    public bool IsAvailable => true;

    public NoOpSessionFeature()
    {
        Session = this;
    }

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public bool TryGetValue(string key, out byte[] value)
    {
        value = [];
        return false;
    }

    public void Set(string key, byte[] value)
    {
    }

    public void Remove(string key)
    {
    }

    public void Clear()
    {
    }
}