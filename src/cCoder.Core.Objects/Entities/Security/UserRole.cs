using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security
{
    [Table("UserRoles", Schema = "Security")]
    public class UserRole
    {
        [ForeignKey("Role")]
        public Guid RoleId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        public virtual Role Role { get; set; }
    }
}