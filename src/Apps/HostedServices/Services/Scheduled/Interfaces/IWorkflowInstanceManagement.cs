namespace HostedServices.Services.Scheduled.Interfaces;

public interface IWorkflowInstanceManagement
{
    ValueTask ExecuteWaitingQueuedInstanceById(Guid id);
    dynamic[] GetStats();
}