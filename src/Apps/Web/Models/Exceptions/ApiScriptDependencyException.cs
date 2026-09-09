// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptDependencyException(
    Exception innerException)
    : Exception("An API script dependency failed.", innerException);