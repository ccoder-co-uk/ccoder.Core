using cCoder.Core.Objects.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Packaging;

[Table("PackageItems", Schema = "Packaging")]
public class PackageItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey("Package")]
    public Guid PackageId { get; set; }

    // used to determine T for data
    public string Type { get; set; }

    // JSON of the object being imported
    public string Data { get; set; }

    public virtual Package Package { get; set; }

    [DontPrivilege]
    public T Unpack<T>() => Objects.Data.ParseJson<T>(Data);
}