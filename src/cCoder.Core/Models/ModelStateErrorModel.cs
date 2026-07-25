// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models;

public class ModelStateErrorModel
{
    public string Key { get; set; }
    public object Value { get; set; }
    public string[] Errors { get; set; }
}