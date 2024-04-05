using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Dtos
{
    public class DataItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
