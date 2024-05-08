namespace cCoder.Core.Api.Hubs;

public class WorkflowHub : CoreHub
{
    private readonly ILogger log;

    public WorkflowHub(ILogger<WorkflowHub> log) : base(log)
    {
        this.log = log;
    }

    public override async Task ConsoleSend(string level, string message, string thread)
    {
        switch (level)
        {
            case "success": 
                log.LogInformation($"{thread}: {level} {message}"); 
                break;

            case "info": 
                log.LogInformation($"{thread}: {level} {message}"); 
                break;

            case "debug": 
                log.LogDebug($"{thread}: {level} {message}"); 
                break;

            case "warn": 
                log.LogWarning($"{thread}: {level} {message}"); 
                break;

            case "error": 
                log.LogError($"{thread}: {level} {message}"); 
                break;
        }

        await base.ConsoleSend(level, message, thread);
    }
}