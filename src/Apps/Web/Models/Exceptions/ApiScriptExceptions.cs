// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptValidationException(
    Exception innerException)
    : Exception("API script validation failed.", innerException);

internal sealed class ApiScriptDependencyException(
    Exception innerException)
    : Exception("An API script dependency failed.", innerException);

internal sealed class ApiScriptServiceException(
    Exception innerException)
    : Exception("The API script service failed.", innerException);

internal sealed class ApiScriptOrchestrationValidationException(
    Exception innerException)
    : Exception("API script orchestration validation failed.", innerException);

internal sealed class ApiScriptOrchestrationDependencyException(
    Exception innerException)
    : Exception("An API script orchestration dependency failed.", innerException);

internal sealed class ApiScriptOrchestrationServiceException(
    Exception innerException)
    : Exception("The API script orchestration failed.", innerException);