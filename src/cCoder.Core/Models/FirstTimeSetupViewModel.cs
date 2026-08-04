// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class FirstTimeSetupViewModel
{
    public string AssetsRoot { get; set; }

    public string Domain { get; set; }

    public FirstTimeSetupRequest Setup { get; set; }
}