using HostedServices.Services.Scheduled.Interfaces;
using Timer = System.Timers.Timer;

namespace HostedServices.Services.Scheduled;

public class SchedulerHostedService(IServiceProvider services, ILogger<SchedulerHostedService> log) : IHostedService
{
    private readonly Timer minuteTimer = new();
    private readonly Timer hourlyTimer = new();
    private readonly Timer dailyTimer = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //If we're running this as part of a pipeline run and don't wish to run scheduled tasks and only care about migrations
        if(int.TryParse(Environment.GetEnvironmentVariable("MIGRATING"), out int result) && result == 1)
            return Task.CompletedTask;

        if (!minuteTimer.Enabled)
        {
            minuteTimer.Interval = 60000;
            minuteTimer.Elapsed += RunMinutelyTasks;
            minuteTimer.Start();
        }

        if (!hourlyTimer.Enabled)
        {
            hourlyTimer.Interval = 3600000;
            hourlyTimer.Elapsed += RunHourlyTasks;
            hourlyTimer.Start();
        }

        if (!dailyTimer.Enabled)
        {
            dailyTimer.Interval = 86400000;
            dailyTimer.Elapsed += RunDailyTasks;
            dailyTimer.Start();
        }

        return Task.CompletedTask;
    }

    private async void RunMinutelyTasks(object sender, System.Timers.ElapsedEventArgs e)
    {
        using IServiceScope scope = services.CreateScope();
        IEnumerable<IScheduled1MinuteOperation> tasks = scope.ServiceProvider.GetRequiredService<IEnumerable<IScheduled1MinuteOperation>>();

        log.LogInformation("Running Minutely services");

        foreach (IScheduled1MinuteOperation s in tasks)
        {
            try
            {
                log.LogDebug($"   Running service {s.GetType().Name}");
                await s.Run();
                log.LogDebug($"   Running service {s.GetType().Name} complete.");
            }
            catch (Exception ex)
            {
                LogException(ex, s);
            }
        }

        log.LogInformation("Minutely services complete.");
    }

    private async void RunHourlyTasks(object sender, System.Timers.ElapsedEventArgs e)
    {
        using IServiceScope scope = services.CreateScope();
        IEnumerable<IScheduledHourlyOperation> tasks = scope.ServiceProvider.GetRequiredService<IEnumerable<IScheduledHourlyOperation>>();

        log.LogInformation("Running Hourly services");

        foreach (IScheduledHourlyOperation s in tasks)
        {
            try
            {
                log.LogDebug($"   Running service {s.GetType().Name}");
                await s.Run();
                log.LogDebug($"   Running service {s.GetType().Name} complete.");
            }
            catch (Exception ex)
            {
                LogException(ex, s);
            }
        }

        log.LogInformation("Hourly services complete.");
    }

    private async void RunDailyTasks(object sender, System.Timers.ElapsedEventArgs e)
    {
        using IServiceScope scope = services.CreateScope();
        IEnumerable<IScheduledDailyOperation> tasks = scope.ServiceProvider.GetRequiredService<IEnumerable<IScheduledDailyOperation>>();

        log.LogInformation("Running Daily services");

        foreach (IScheduledDailyOperation s in tasks)
        {
            try
            {
                log.LogDebug($"   Running service {s.GetType().Name}");
                await s.Run();
                log.LogDebug($"   Running service {s.GetType().Name} complete.");
            }
            catch (Exception ex)
            {
                LogException(ex, s);
            }
        }

        log.LogInformation("Daily services complete.");
    }

    private void LogException(Exception ex, object task)
    {
        log.LogError($"Exception caught in {task.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

        if (ex.InnerException != null)
            LogException(ex.InnerException, task);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (minuteTimer.Enabled)
        {
            minuteTimer.Stop();
            minuteTimer.Elapsed -= RunMinutelyTasks;
        }

        if (hourlyTimer.Enabled)
        {
            hourlyTimer.Stop();
            hourlyTimer.Elapsed -= RunHourlyTasks;
        }

        if (dailyTimer.Enabled)
        {
            dailyTimer.Stop();
            dailyTimer.Elapsed -= RunDailyTasks;
        }
        return Task.CompletedTask;
    }
}