// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class FirstTimeSetupViewModel
{
    public string Domain { get; set; } = string.Empty;

    public FirstTimeSetupRequest Setup { get; set; } = new();
}