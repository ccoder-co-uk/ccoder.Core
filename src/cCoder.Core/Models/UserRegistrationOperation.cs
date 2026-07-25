// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;

namespace cCoder.Core.Models;

internal sealed class UserRegistrationOperation
{
    public UserRegistrationOperationType Type { get; set; }

    public string RegistrationToken { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public RegisterUser Registration { get; set; }

    public Token AuthenticationToken { get; set; }

    public SSOUser User { get; set; }
}