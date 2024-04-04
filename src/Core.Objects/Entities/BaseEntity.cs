using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Entities
{
    public class BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(350)]
        public string Description { get; set; }

        [DefaultValue(typeof(DateTime), "2021-02-19")]
        public DateTimeOffset LastUpdated { get; set; }

        [StringLength(100)]
        public string LastUpdatedBy { get; set; }

        [DefaultValue(typeof(DateTime), "2021-02-19")]
        public DateTimeOffset CreatedOn { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }
    }
}