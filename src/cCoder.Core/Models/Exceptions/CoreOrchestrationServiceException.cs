// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationServiceException(Exception innerException)
    : Exception("The Core orchestration service failed.", innerException);