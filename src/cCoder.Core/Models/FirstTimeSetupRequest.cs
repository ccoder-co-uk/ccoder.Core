// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class FirstTimeSetupRequest
{
    public FirstTimeSetupRequest()
    {
        Domain = string.Empty;
        TenantName = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    public string Domain { get; set; }

    public string TenantName { get; set; }

    public string DisplayName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string ConfirmPassword { get; set; }
}