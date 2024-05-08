namespace cCoder.Core.Objects.Dtos.Workflow;

public class WorkflowRequest
{
    public string Api { get; set; }

    public Guid FlowId { get; set; }

    public Guid InstanceId { get; set; }

    public string AuthToken { get; set; }

    public WorkflowRequest() { }

    public WorkflowRequest(string api, string token, Guid flowId, Guid instanceId)
    {
        Api = api;
        AuthToken = token;
        FlowId = flowId;
        InstanceId = instanceId;
    }
}