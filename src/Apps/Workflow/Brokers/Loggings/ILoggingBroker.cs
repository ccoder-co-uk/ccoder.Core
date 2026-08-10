// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Workflow.Brokers.Loggings;

internal interface ILoggingBroker
{
    void LogInformation(string message, params object[] args);
}