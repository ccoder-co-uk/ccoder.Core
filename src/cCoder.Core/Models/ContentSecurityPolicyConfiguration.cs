// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public sealed class ContentSecurityPolicyConfiguration
{
    public string[] ConnectSources { get; set; }
    public string[] FontSources { get; set; }
    public string[] FrameSources { get; set; }
    public string[] ImageSources { get; set; }
    public string[] ScriptSources { get; set; }
    public string[] StyleSources { get; set; }
}