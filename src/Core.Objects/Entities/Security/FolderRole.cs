using cCoder.Core.Objects.Entities.DMS;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace cCoder.Core.Objects.Entities.Security
{
    [Table("FolderRoles", Schema = "Security")]
    public class FolderRole
    {
        [ForeignKey("Folder")]
        public Guid FolderId { get; set; }

        [ForeignKey("Role")]
        public Guid RoleId { get; set; }

        public virtual Folder Folder { get; set; }

        public virtual Role Role { get; set; }
    }
}