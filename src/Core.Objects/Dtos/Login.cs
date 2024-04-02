using System.ComponentModel.DataAnnotations;

namespace Core.Objects.Dtos
{
    public class Login
    {
        [Required]
        [Display(Name = "User")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}
