// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreProcessingDependencyException(Exception innerException)
    : Exception("A Core processing dependency failed.", innerException);