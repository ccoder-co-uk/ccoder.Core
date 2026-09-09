// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationServiceException(Exception innerException)
    : Exception("The Core orchestration service failed.", innerException);