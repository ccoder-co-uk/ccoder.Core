// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Metadata;

internal sealed class EdmModelType
{
    public Type ClrType { get; set; }
    public bool HasEndpoint { get; set; }
}