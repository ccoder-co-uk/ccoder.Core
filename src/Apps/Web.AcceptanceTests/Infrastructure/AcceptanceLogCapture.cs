// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Web.AcceptanceTests.Infrastructure;

#pragma warning disable STXTEST001
internal sealed class AcceptanceLogCapture : ILoggerProvider
{
    private readonly ConcurrentQueue<string> entries = new();

    internal string Read() =>
        string.Join(
            separator: Environment.NewLine,
            values: entries.ToArray());

    public ILogger CreateLogger(string categoryName) =>
        new CaptureLogger(categoryName: categoryName, entries: entries);

    public void Dispose()
    { }

    private sealed class CaptureLogger(
        string categoryName,
        ConcurrentQueue<string> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull =>
            NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= LogLevel.Error;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel: logLevel))
            {
                return;
            }

            entries.Enqueue(
                item: $"{categoryName}: {formatter(arg1: state, arg2: exception)}{Environment.NewLine}{exception}");
        }
    }

    private sealed class NullScope : IDisposable
    {
        internal static NullScope Instance { get; } = new();

        public void Dispose()
        { }
    }
}
#pragma warning restore STXTEST001