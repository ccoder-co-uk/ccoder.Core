using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities;

[Table("CommonObjects", Schema = "CMS")]
public class CommonObject : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int Version { get; set; }

    public string Key { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public string Json { get; set; }

    public string Culture { get; set; }
}