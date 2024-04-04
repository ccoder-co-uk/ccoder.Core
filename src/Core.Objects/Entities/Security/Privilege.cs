using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security
{
    [Table("Privileges", Schema = "Security")]
    public class Privilege
    {
        [Key]
        [StringLength(200)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Operation { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public bool PortalAdminsOnly { get; set; }
    }
}