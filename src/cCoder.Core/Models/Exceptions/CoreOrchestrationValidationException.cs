// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationValidationException(Exception innerException)
    : Exception("Core orchestration validation failed.", innerException);