// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Metadata;

internal sealed class EdmModelOperation
{
    public string Name { get; set; }
    public bool IsFunction { get; set; }
    public bool ReturnIsCollection { get; set; }
    public string ReturnTypeName { get; set; }
    public IDictionary<string, string> Parameters { get; set; }
}