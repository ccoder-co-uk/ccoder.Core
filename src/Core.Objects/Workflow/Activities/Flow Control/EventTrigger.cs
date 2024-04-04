namespace cCoder.Core.Objects.Workflow.Activities
{
    /// <summary>
    /// Root node for any flow, you MUST begin with this node and a given flow can ONLY contain one of these.
    /// </summary>
    public sealed class EventTrigger<T> : Activity
    {
        public string Api { get; set; }
        public string AuthToken { get; set; }
        public T Data { get; set; }
    }
}