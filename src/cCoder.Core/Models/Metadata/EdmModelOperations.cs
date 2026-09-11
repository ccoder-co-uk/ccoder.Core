// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Metadata;

internal sealed class EdmModelOperations
{
    public bool HasEntitySet { get; set; }
    public IReadOnlyCollection<EdmModelOperation> Operations { get; set; }
}