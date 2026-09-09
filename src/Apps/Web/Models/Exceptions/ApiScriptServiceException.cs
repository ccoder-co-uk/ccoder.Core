// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiScriptServiceException(
    Exception innerException)
    : Exception("The API script service failed.", innerException);