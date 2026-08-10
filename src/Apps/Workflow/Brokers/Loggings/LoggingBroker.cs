// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Workflow.Brokers.Loggings;

internal sealed class LoggingBroker(ILogger<LoggingBroker> logger)
    : ILoggingBroker
{
    public void LogInformation(string message, params object[] args) =>
        logger.LogInformation(message: message, args: args);
}