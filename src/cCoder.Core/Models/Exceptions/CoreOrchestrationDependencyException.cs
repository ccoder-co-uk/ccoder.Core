// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationDependencyException(Exception innerException)
    : Exception("A Core orchestration dependency failed.", innerException);