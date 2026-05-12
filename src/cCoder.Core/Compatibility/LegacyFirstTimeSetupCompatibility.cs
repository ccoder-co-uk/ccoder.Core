using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public sealed class FirstTimeSetupRequest
    {
        public string Domain { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tenant name")]
        public string TenantName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Display name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public sealed class FirstTimeSetupViewModel
    {
        public string Domain { get; set; } = string.Empty;

        public FirstTimeSetupRequest Setup { get; set; } = new();
    }
}

namespace Web.Services.Setup
{
    public interface IFirstTimeSetupStateService
    {
        Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
    }

    internal sealed class LegacyFirstTimeSetupStateServiceAdapter(
        cCoder.Core.Services.Setup.IFirstTimeSetupStateService inner)
        : IFirstTimeSetupStateService
    {
        public Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default) =>
            inner.IsInitializedAsync(cancellationToken);
    }

    public static class SetupRequestHostNormalizer
    {
        public static string Normalize(string host) =>
            cCoder.Core.Services.Setup.SetupRequestHostNormalizer.Normalize(host);
    }
}
