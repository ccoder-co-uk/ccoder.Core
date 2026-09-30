// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Models.Exceptions;

internal sealed class ApiScriptOrchestrationServiceException(
    Exception innerException)
    : Exception("The API script orchestration failed.", innerException);