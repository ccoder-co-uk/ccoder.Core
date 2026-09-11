// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Metadata;

internal sealed class EdmModelDetails
{
    public object Model { get; set; }
    public Type Type { get; set; }
    public IReadOnlyCollection<EdmModelType> Types { get; set; }
    public EdmModelOperations Operations { get; set; }
}