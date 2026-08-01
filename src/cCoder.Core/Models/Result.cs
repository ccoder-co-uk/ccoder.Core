// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Models;

public class Result
{
    [Key]
    public virtual string Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}