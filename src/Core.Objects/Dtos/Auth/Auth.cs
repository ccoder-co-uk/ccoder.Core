using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Dtos.Auth
{
    public class Auth
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }

    public class RegisterUser
    {
        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [PasswordAttribute(ErrorMessage = "Does not meet the complexity requirements.")]
        public string Password { get; set; }

        public string Culture { get; set; }

        public string PhoneNumber { get; set; }

        public int? AppId { get; set; }
    }
}