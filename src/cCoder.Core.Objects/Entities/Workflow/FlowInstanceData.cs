using cCoder.Core.Objects.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Workflow;

[Table("FlowInstances", Schema = "Workflow")]
[Parent("FlowDefinition")]
public class FlowInstanceData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [ForeignKey("FlowDefinition")]
    public Guid FlowDefinitionId { get; set; }

    public string Name { get; set; }

    [ApiIgnore]
    [JsonIgnore]
    public byte[] ContextJson { get; set; }

    public string State { get; set; }

    public string ReportingComponentName { get; set; }

    public string Caller { get; set; }

    public string ContextString
    {
        get => ContextJson != null ? System.Text.Encoding.UTF8.GetString(ContextJson) : string.Empty;
        set => ContextJson = value != null ? System.Text.Encoding.UTF8.GetBytes(value) : null;
    }

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset? End { get; set; }

    public virtual FlowDefinition FlowDefinition { get; set; }
}