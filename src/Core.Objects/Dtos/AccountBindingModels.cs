using System.ComponentModel.DataAnnotations;

namespace Core.Objects.Dtos
{
    public class ChangePassword : SetPassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    public class SetPassword
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [PasswordAttribute(ErrorMessage = "Does not meet the complexity requirements.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ConfirmForgotPassword : SetPassword
    {
        public string UserId { get; set; }
        public int SourceAppId { get; set; }

        [Required]
        public string Token { get; set; }
    }

    public class ForgotPass
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int AppId { get; set; }
    }

    public class ConfirmEmail
    {
        public string SSOUserId { get; set; }
        public string Token { get; set; }
    }
}