// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptOrchestrationValidationException(
    Exception innerException)
    : Exception("API script orchestration validation failed.", innerException);