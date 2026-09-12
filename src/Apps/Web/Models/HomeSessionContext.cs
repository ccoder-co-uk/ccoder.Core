// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models;

internal sealed class HomeSessionContext
{
    public string Host { get; set; }
    public int? Port { get; set; }
    public string Scheme { get; set; }
    public string SSOUserId { get; set; }
    public string Token { get; set; }
    public string[] SessionKeys { get; set; }
}