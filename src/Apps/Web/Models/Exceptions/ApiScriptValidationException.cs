// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptValidationException(
    Exception innerException)
    : Exception("API script validation failed.", innerException);