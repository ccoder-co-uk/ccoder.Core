// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationValidationException(Exception innerException)
    : Exception("Core orchestration validation failed.", innerException);