namespace HostedServices.Services.Scheduled.Interfaces;

public interface IScheduledOperation
{
    Task Run();
}

public interface IScheduled1MinuteOperation : IScheduledOperation { }

public interface IScheduledHourlyOperation : IScheduledOperation { }

public interface IScheduledDailyOperation : IScheduledOperation { }