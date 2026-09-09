// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptOrchestrationDependencyException(
    Exception innerException)
    : Exception("An API script orchestration dependency failed.", innerException);