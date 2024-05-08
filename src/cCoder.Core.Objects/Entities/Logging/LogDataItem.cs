using cCoder.Core.Objects.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Logging;

[Table("LogDataItems", Schema = "Logging")]
[ApiIgnore]
public class LogDataItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("LogEntry")]
    public int LogEntryId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Value { get; set; }

    public virtual LogEntry LogEntry { get; set; }
}
