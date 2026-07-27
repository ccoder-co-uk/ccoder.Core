// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class FirstTimeSetupViewModel
{
    public FirstTimeSetupViewModel()
    {
        AssetsRoot = string.Empty;
        Domain = string.Empty;
        Setup = new FirstTimeSetupRequest();
    }

    public string AssetsRoot { get; set; }

    public string Domain { get; set; }

    public FirstTimeSetupRequest Setup { get; set; }
}